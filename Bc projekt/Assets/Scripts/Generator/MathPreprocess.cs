using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace Assets.Scripts.Generator
{
    /// <summary>
    /// Core mathematical engine. 
    /// Handles tokenization, Shunting-Yard (Infix to Postfix) conversion, 
    /// and JIT compilation of strings into .NET delegates using Expression Trees.
    /// </summary>
    public class MathPreprocess
    {
        private static bool IsFunction(string token)
        {
            return token == "sin" || token == "cos" || token == "tan" ||
                   token == "sqrt" || token == "log" || token == "exp" || token == "abs"; 
        }

        private static readonly Dictionary<string, int> priority = new Dictionary<string, int>
        {
            { "+", 1 },
            { "-", 1 },
            { "*", 2 },
            { "/", 2 },
            { "^", 3 }
        };

        /// <summary>
        /// Converts a raw string into a list of mathematical tokens.
        /// </summary>
        public static List<string> Tokenize(string input)
        {
            input = input.Replace(" ", "").Replace(',', '.').ToLower();
            List<string> tokens = new List<string>();

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (char.IsDigit(c) || c == '.')
                {
                    string number = "";
                    int j = i;
                    while (j < input.Length && (char.IsDigit(input[j]) || input[j] == '.'))
                    {
                        number += input[j];
                        j++;
                    }
                    tokens.Add(number);
                    i = j - 1;
                }
                else if ("+-*/^()".Contains(c))
                {
                    // Handle unary minus by prepending a 0
                    if (c == '-' && (tokens.Count == 0 || tokens[tokens.Count - 1] == "("))
                    {
                        tokens.Add("0");
                    }
                    tokens.Add(c.ToString());
                }
                else if (char.IsLetter(c))
                {
                    string word = "";
                    int j = i;
                    while (j < input.Length && char.IsLetter(input[j]))
                    {
                        word += input[j];
                        j++;
                    }

                    bool hasParenthesisNext = (j < input.Length && input[j] == '(');

                    if (IsFunction(word))
                    {
                        if (!hasParenthesisNext) throw new Exception($"Missing '(' after function '{word}'.");
                        tokens.Add(word);
                    }
                    else
                    {
                        if (hasParenthesisNext) throw new Exception($"Unknown function '{word}'.");
                        tokens.Add(word);
                    }
                    i = j - 1;
                }
                else throw new Exception($"Unknown character: '{c}'");
            }
            return tokens;
        }

        /// <summary>
        /// Implements the Shunting-Yard algorithm to convert infix notation to Reverse Polish Notation (RPN).
        /// </summary>
        public static List<string> ShuntingYard(List<string> input) {
            List<string> outputQueue = new List<string>();
            Stack<string> operatorStack = new Stack<string>();

            foreach (string token in input)
            {
                if (char.IsDigit(token[0]) || token[0] == '.' || (char.IsLetter(token[0]) && !IsFunction(token)))
                {
                    outputQueue.Add(token);
                }
                else if (IsFunction(token))
                {
                    operatorStack.Push(token);
                }
                else if (priority.ContainsKey(token))
                {
                    while (operatorStack.Count > 0 && priority.ContainsKey(operatorStack.Peek()))
                    {
                        if (token != "^" && priority[token] <= priority[operatorStack.Peek()])
                            outputQueue.Add(operatorStack.Pop());
                        else break;
                    }
                    operatorStack.Push(token);
                }
                else if (token == "(")
                {
                    operatorStack.Push(token);
                }
                else if (token == ")")
                {
                    while (operatorStack.Count > 0 && operatorStack.Peek() != "(")
                        outputQueue.Add(operatorStack.Pop());
                    
                    if (operatorStack.Count > 0 && operatorStack.Peek() == "(") operatorStack.Pop();
                    if (operatorStack.Count > 0 && IsFunction(operatorStack.Peek())) outputQueue.Add(operatorStack.Pop());
                }
            }
            while (operatorStack.Count > 0) outputQueue.Add(operatorStack.Pop());
            return outputQueue;
        }

        /// <summary>
        /// Compiles a mathematical string into a high-performance delegate.
        /// </summary>
        /// <param name="input">The math string.</param>
        /// <returns>A tuple containing the delegate (f(x, z, args)) and a list of detected parameter names.</returns>
        public static (Func<float, float, float[], float>, List<string>) Compile(string input)
        {
            PreValidateInput(input);
            
            List<string> RPN = ShuntingYard(Tokenize(input));
            
            return BuildExpressionTree(RPN);
        }

        private static void PreValidateInput(string input)
        {
            if (input.Count(c => c == '(') != input.Count(c => c == ')')) 
                throw new Exception("Parentheses mismatch.");
        }

        private static (Func<float, float, float[], float>, List<string>) BuildExpressionTree(List<string> RPN)
        {
            ParameterExpression xParam = Expression.Parameter(typeof(float), "x");
            ParameterExpression yParam = Expression.Parameter(typeof(float), "y");
            ParameterExpression argsParam = Expression.Parameter(typeof(float[]), "args");

            Stack<Expression> stack = new Stack<Expression>();
            List<string> cparams = new List<string>();

            foreach (string token in RPN)
            {
                if (float.TryParse(token, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float number))
                {
                    stack.Push(Expression.Constant(number, typeof(float)));
                }
                else if (token == "x") stack.Push(xParam);
                else if (token == "y") stack.Push(yParam);
                else if (IsFunction(token))
                {
                    if (stack.Count < 1) throw new Exception($"Missing argument for '{token}'.");
                    Expression arg = stack.Pop();
                    string methodName = char.ToUpper(token[0]) + token.Substring(1);
                    MethodInfo method = typeof(UnityEngine.Mathf).GetMethod(methodName, new Type[] { typeof(float) });
                    stack.Push(method != null ? Expression.Call(method, arg) : Expression.Constant(0f, typeof(float)));
                }
                else if (priority.ContainsKey(token))
                {
                    if (stack.Count < 2) throw new Exception($"Missing operand for '{token}'.");
                    Expression right = stack.Pop();
                    Expression left = stack.Pop();

                    switch (token)
                    {
                        case "+": stack.Push(Expression.Add(left, right)); break;
                        case "-": stack.Push(Expression.Subtract(left, right)); break;
                        case "*": stack.Push(Expression.Multiply(left, right)); break;
                        case "/": stack.Push(Expression.Divide(left, right)); break;
                        case "^":
                            MethodInfo powMethod = typeof(UnityEngine.Mathf).GetMethod("Pow", new[] { typeof(float), typeof(float) });
                            stack.Push(Expression.Call(powMethod, left, right));
                            break;
                    }
                }
                else
                {
                    if (!cparams.Contains(token)) cparams.Add(token);
                    int index = cparams.IndexOf(token);
                    stack.Push(Expression.ArrayIndex(argsParam, Expression.Constant(index)));
                }
            }
            if (stack.Count > 1) throw new Exception("Unexpected expression end.");
            
            Expression finalExpression = stack.Pop();
            var compiled = Expression.Lambda<Func<float, float, float[], float>>(finalExpression, xParam, yParam, argsParam).Compile();
            
            return (compiled, cparams);
        }
    }
}
