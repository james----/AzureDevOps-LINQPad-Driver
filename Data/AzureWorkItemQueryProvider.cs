using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AzureDevOpsDataContextDriver
{
    public class AzureWorkItemQueryProvider : IOrderedQueryable<AzureWorkItem>, IQueryProvider
    {
        static Dictionary<string, string> ColumnMapping
        {
            get
            {
                return new Dictionary<string, string>
                {
                    ["Id"] = "System.Id",
                    ["Title"] = "System.Title",
                    ["ItemType"] = "System.WorkItemType",
                    ["State"] = "System.State",
                    ["AssignedTo"] = "System.AssignedTo",
                    ["IterationPath"] = "System.IterationPath",
                    ["AssignedOn"] = "System.AssignedOn",
                    ["Blocked"] = "Microsoft.VSTS.CMMI.Blocked",
                    ["CreatedDate"] = "System.CreatedDate",
                    ["ClosedDate"] = "Microsoft.VSTS.Common.ClosedDate",
                    ["BacklogPriority"] = "Microsoft.VSTS.Common.BacklogPriority"
                };
            }
        }

        AzureDevOpsConnectionInfo ConnectionInfo;

        WorkItemTrackingHttpClient witClient;

        Expression Expression { get; set; }

        public AzureWorkItemQueryProvider(AzureDevOpsConnectionInfo connInfo, WorkItemTrackingHttpClient _witClient)
        {
            witClient = _witClient;
            ConnectionInfo = connInfo;
            Expression = Expression.Constant(this);
        }

        private IQueryable<AzureWorkItem> ExecuteQueryInternal(Expression expression)
        {
            WhereFinder whereFinder = new WhereFinder();
            MethodCallExpression whereExpression = whereFinder.GetWhere(expression);
            if (whereExpression != null)
            {
                LambdaExpression lambdaExpression = (whereExpression.Arguments[1] as UnaryExpression).Operand as LambdaExpression;
                WhereBuilder whereBuilder = new WhereBuilder();
                var whereCond = whereBuilder.ToSql(lambdaExpression as Expression<Func<AzureWorkItem, bool>>);
                return AzureDevOpsWorkItemFactory.GetWorkItems(ConnectionInfo, witClient, whereCond).GetAwaiter().GetResult();
            }
            throw new NotSupportedException("Cannot query without a where condition.");
        }

        #region IQueryProvider
        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
        {
            return ExecuteQueryInternal(expression) as IQueryable<TElement>;
        }

        object IQueryProvider.Execute(Expression expression)
        {
            throw new NotImplementedException();
        }

        TResult IQueryProvider.Execute<TResult>(Expression expression)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IOrderedQueryable
        Expression IQueryable.Expression => Expression;

        Type IQueryable.ElementType => typeof(AzureWorkItem);

        IQueryProvider IQueryable.Provider => (this as IQueryProvider);

        IEnumerator<AzureWorkItem> IEnumerable<AzureWorkItem>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        #endregion

        private class WhereFinder : ExpressionVisitor
        {
            private MethodCallExpression innermostWhereExpression;

            public MethodCallExpression GetWhere(Expression expression)
            {
                Visit(expression);
                return innermostWhereExpression;
            }

            protected override Expression VisitMethodCall(MethodCallExpression expression)
            {
                if (expression.Method.Name == "Where")
                    innermostWhereExpression = expression;
                Visit(expression.Arguments[0]);
                return expression;
            }
        }

        private class WhereBuilder
        {
            public string ToSql<T>(Expression<Func<T, bool>> expression)
            {
                return Recurse(expression.Body, true);
            }

            private string Recurse(Expression expression, bool isUnary = false, bool quote = true)
            {
                if (expression is UnaryExpression)
                {
                    var unary = (UnaryExpression)expression;
                    var right = Recurse(unary.Operand, true);
                    return "(" + NodeTypeToString(unary.NodeType, right == "NULL") + " " + right + ")";
                }
                if (expression is BinaryExpression)
                {
                    var body = (BinaryExpression)expression;
                    var right = Recurse(body.Right);
                    return "(" + Recurse(body.Left) + " " + NodeTypeToString(body.NodeType, right == "NULL") + " " + right + ")";
                }
                if (expression is ConstantExpression)
                {
                    var constant = (ConstantExpression)expression;
                    return ValueToString(constant.Value, isUnary, quote);
                }
                if (expression is MemberExpression)
                {
                    var member = (MemberExpression)expression;

                    if (member.Member is PropertyInfo)
                    {
                        var property = (PropertyInfo)member.Member;
                        var colName = ColumnMapping[property.Name];
                        if (isUnary && member.Type == typeof(bool))
                        {
                            return "([" + colName + "] = 1)";
                        }
                        return "[" + colName + "]";
                    }
                    if (member.Member is FieldInfo)
                    {
                        return ValueToString(GetValue(member), isUnary, quote);
                    }
                    throw new InvalidQueryException($"Expression does not refer to a property or field: {expression}");
                }
                if (expression is MethodCallExpression)
                {
                    var methodCall = (MethodCallExpression)expression;
                    // LIKE queries:
                    if (methodCall.Method == typeof(string).GetMethod("Contains", new[] { typeof(string) }))
                    {
                        return "(" + Recurse(methodCall.Object) + " LIKE '%" + Recurse(methodCall.Arguments[0], quote: false) + "%')";
                    }
                    if (methodCall.Method == typeof(string).GetMethod("StartsWith", new[] { typeof(string) }))
                    {
                        return "(" + Recurse(methodCall.Object) + " LIKE '" + Recurse(methodCall.Arguments[0], quote: false) + "%')";
                    }
                    if (methodCall.Method == typeof(string).GetMethod("EndsWith", new[] { typeof(string) }))
                    {
                        return "(" + Recurse(methodCall.Object) + " LIKE '%" + Recurse(methodCall.Arguments[0], quote: false) + "')";
                    }
                    // IN queries:
                    if (methodCall.Method.Name == "Contains")
                    {
                        Expression collection;
                        Expression property;
                        if (methodCall.Method.IsDefined(typeof(ExtensionAttribute)) && methodCall.Arguments.Count == 2)
                        {
                            collection = methodCall.Arguments[0];
                            property = methodCall.Arguments[1];
                        }
                        else if (!methodCall.Method.IsDefined(typeof(ExtensionAttribute)) && methodCall.Arguments.Count == 1)
                        {
                            collection = methodCall.Object;
                            property = methodCall.Arguments[0];
                        }
                        else
                        {
                            throw new InvalidQueryException("Unsupported method call: " + methodCall.Method.Name);
                        }
                        var values = (IEnumerable)GetValue(collection);
                        var concated = "";
                        foreach (var e in values)
                        {
                            concated += ValueToString(e, false, true) + ", ";
                        }
                        if (concated == "")
                        {
                            return ValueToString(false, true, false);
                        }
                        return "(" + Recurse(property) + " IN (" + concated.Substring(0, concated.Length - 2) + "))";
                    }
                    //SUB QUERIES
                    if (methodCall.Method.Name == "Any")
                    {
                        throw new InvalidQueryException("Unsupported method call: " + methodCall.Method.Name);
                    }
                    throw new InvalidQueryException("Unsupported method call: " + methodCall.Method.Name);
                }
                throw new InvalidQueryException("Unsupported expression: " + expression.GetType().Name);
            }

            public string ValueToString(object value, bool isUnary, bool quote)
            {
                if (value == null) throw new InvalidQueryException("Query contains expression with null");
                if (value is bool)
                {
                    if (isUnary)
                    {
                        return (bool)value ? "(1=1)" : "(1=0)";
                    }
                    return (bool)value ? "1" : "0";
                }
                else if (value is DateTime)
                {
                    return $"'{ ((DateTime)value).ToString("MM/dd/yyyy HH:mm:ss") }'";
                }
                return quote ? $"'{ value }'" : value.ToString();
            }

            private static bool IsEnumerableType(Type type)
            {
                return type
                    .GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            }

            private static object GetValue(Expression member)
            {
                var objectMember = Expression.Convert(member, typeof(object));
                var getterLambda = Expression.Lambda<Func<object>>(objectMember);
                var getter = getterLambda.Compile();
                return getter();
            }

            private static object NodeTypeToString(ExpressionType nodeType, bool rightIsNull)
            {
                switch (nodeType)
                {
                    case ExpressionType.Add:
                        return "+";
                    case ExpressionType.And:
                        return "&";
                    case ExpressionType.AndAlso:
                        return "AND";
                    case ExpressionType.Divide:
                        return "/";
                    case ExpressionType.Equal:
                        return rightIsNull ? "IS" : "=";
                    case ExpressionType.ExclusiveOr:
                        return "^";
                    case ExpressionType.GreaterThan:
                        return ">";
                    case ExpressionType.GreaterThanOrEqual:
                        return ">=";
                    case ExpressionType.LessThan:
                        return "<";
                    case ExpressionType.LessThanOrEqual:
                        return "<=";
                    case ExpressionType.Modulo:
                        return "%";
                    case ExpressionType.Multiply:
                        return "*";
                    case ExpressionType.Negate:
                        return "-";
                    case ExpressionType.Not:
                        return "NOT";
                    case ExpressionType.NotEqual:
                        return "<>";
                    case ExpressionType.Or:
                        return "|";
                    case ExpressionType.OrElse:
                        return "OR";
                    case ExpressionType.Subtract:
                        return "-";
                }
                throw new InvalidQueryException($"Unsupported node type: {nodeType}");
            }
        }

        private class ExpressionTreeHelpers
        {
            internal static bool IsMemberEqualsValueExpression(Expression exp, Type declaringType, string memberName)
            {
                if (exp.NodeType != ExpressionType.Equal)
                    return false;

                BinaryExpression be = (BinaryExpression)exp;

                if (IsSpecificMemberExpression(be.Left, declaringType, memberName) && IsSpecificMemberExpression(be.Right, declaringType, memberName))
                    throw new Exception("Cannot have 'member' == 'member' in an expression!");

                return (IsSpecificMemberExpression(be.Left, declaringType, memberName) ||
                    IsSpecificMemberExpression(be.Right, declaringType, memberName));
            }

            internal static bool IsSpecificMemberExpression(Expression exp, Type declaringType, string memberName)
            {
                return ((exp is MemberExpression) &&
                    (((MemberExpression)exp).Member.DeclaringType == declaringType) &&
                    (((MemberExpression)exp).Member.Name == memberName));
            }

            internal static string GetValueFromEqualsExpression(BinaryExpression be, Type memberDeclaringType, string memberName)
            {
                if (be.NodeType != ExpressionType.Equal)
                    throw new Exception("There is a bug in this program.");

                if (be.Left.NodeType == ExpressionType.MemberAccess)
                {
                    MemberExpression me = (MemberExpression)be.Left;

                    if (me.Member.DeclaringType == memberDeclaringType && me.Member.Name == memberName)
                    {
                        return GetValueFromExpression(be.Right);
                    }
                }
                else if (be.Right.NodeType == ExpressionType.MemberAccess)
                {
                    MemberExpression me = (MemberExpression)be.Right;

                    if (me.Member.DeclaringType == memberDeclaringType && me.Member.Name == memberName)
                    {
                        return GetValueFromExpression(be.Left);
                    }
                }
                throw new Exception("There is a bug in this program.");
            }

            internal static string GetValueFromExpression(Expression expression)
            {
                if (expression.NodeType == ExpressionType.Constant)
                    return (string)(((ConstantExpression)expression).Value);
                else
                    throw new InvalidQueryException(string.Format("The expression type {0} is not supported to obtain a value.", expression.NodeType));
            }
        }

        private class InvalidQueryException : Exception
        {
            private string message;

            public InvalidQueryException(string message)
            {
                this.message = message + " ";
            }

            public override string Message
            {
                get
                {
                    return "The client query is invalid: " + message;
                }
            }
        }
    }
}
