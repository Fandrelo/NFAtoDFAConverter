using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Automatas.Homebrew;

namespace Automatas.POCO
{
    public class FiveTuple
    {
        public static string LAMBDA = "e";
        public static int ASCII_A = 65;
        public string[] Q { get; set; }
        public string[] F { get; set; }
        public string I { get; set; }
        public string[] A { get; set; }
        private Transition[] W { get; set; }
        public string[] Comps { get; set; }
        private string CurrentNodeOnGraph { get; set; } = string.Empty;
        public bool isValid { get; set; } = true;
        public FiveTuple() { }
        public FiveTuple(string rawData)
        {
            rawData = rawData.Replace("\r", "");
            var splittedData = rawData.Split('\n');
            var ads = splittedData[0].Substring(splittedData[0].Length - 2);
            isValid = (splittedData[0].Substring(0, 3) == "Q={" && splittedData[0].Substring(splittedData[0].Length - 1) == "}" &&
                splittedData[1].Substring(0, 3) == "F={" && splittedData[1].Substring(splittedData[1].Length - 1) == "}" &&
                splittedData[2].Substring(0, 2) == "i=" &&
                splittedData[3].Substring(0, 2) == "A=" &&
                splittedData[4].Substring(0, 3) == "W={") && splittedData[4].Substring(splittedData[4].Length - 2) == ")}";
            Q = Regex.Match(splittedData[0], @"(?<=\{).*(?=\})").ToString().Split(',');
            F = Regex.Match(splittedData[1], @"(?<=\{).*(?=\})").ToString().Split(',');
            I = Regex.Match(splittedData[2], @"(?<=i=)\w*").ToString();
            A = new []{ Regex.Match(splittedData[3], @"(?<=A=)\w*").ToString() };
            if (string.IsNullOrEmpty(A[0]))
            {
                A = Regex.Match(splittedData[3], @"(?<=\{).*(?=\})").ToString().Split(',');
            }
            var regexW = Regex.Matches(splittedData[4], @"(?<=\().+?(?=\))");
            W = new Transition[regexW.Count];
            for (int i = 0; i < regexW.Count; i++)
            {
                var transitionString = regexW[i].ToString().Split(',');
                W[i] = new Transition(transitionString[0], transitionString[1], transitionString[2]);
            }
        }
        public string Find(string state, string symbol)
        {
            foreach(var a in W)
            {
                if(a.Find(state, symbol))
                {
                    return a.End;
                }
            }
            return string.Empty;
        }
        /// <summary>
        /// Matriz AFN
        /// </summary>
        public List<List<string>> toOutputMatrix()
        {
            var matrix = new List<List<string>>();

            for (int i = 0; i < Q.Count() + 1; i++)
            {
                matrix.Add(new List<string>());

                for (int j = 0; j < F.Count() + 1; j++)
                {
                    if (i == 0 && j == 0)
                    {
                        matrix[i].Add("Estados");
                    }
                    else if (i == 0)
                    {
                        matrix[i].Add(F[j - 1]);
                    }
                    else if (j == 0)
                    {
                        matrix[i].Add(Q[i - 1]);
                    }
                    else
                    {
                        matrix[i].Add(Find(Q[i - 1], F[j - 1]));
                    }
                }
            }
            return matrix;
        }
        public string toGraph()
        {
            var timeOnStart = DateTime.Now.ToString("yyyyMMddHHmmssffff");
            timeOnStart += ".dot";
            var dot = "";
            dot += "digraph G {" +
                    "rankdir=\"LR\";\n" +
                    "node [shape = none] i;\n";

            bool flag;
            foreach (var i in Q)
            {
                flag = false;
                foreach (var j in A)
                {
                    if (i == j)
                    {
                        flag = true;
                        break;
                    }
                }
                if(flag)
                {
                    if(i.Equals(CurrentNodeOnGraph))
                    {
                        dot += "node [shape = doublecircle color = blue] " + i + ";\n";
                    }
                    else
                    {
                        dot += "node [shape = doublecircle color = black] " + i + ";\n";
                    }
                } else
                {
                    if (i.Equals(CurrentNodeOnGraph))
                    {
                        dot += "node [shape = circle color = blue] " + i + ";\n";
                    }
                    else
                    {
                        dot += "node [shape = circle color = black] " + i + ";\n";
                    }
                }
            }
            dot += "i -> " + Q[0] + ";\n";
            foreach (var a in W)
            {
                switch (a.EdgeColorStatus)
                {
                    case -1:
                    {
                        dot += a.Begin + " -> " + a.End + "[label = \" " + a.Symbol + "\"];\n";
                        break;
                    }
                    case 0:
                    {
                        dot += a.Begin + " -> " + a.End + "[label = \" " + a.Symbol + "\" color = indianred1];\n";
                        break;
                    }
                    case 1:
                    {
                        dot += a.Begin + " -> " + a.End + "[label = \" " + a.Symbol + "\" color = darkolivegreen1];\n";
                        break;
                    }
                    case 2:
                    {
                        dot += a.Begin + " -> " + a.End + "[label = \" " + a.Symbol + ":" + a.EdgeTimesUsed.ToString() + "\" color = indianred4];\n";
                        break;
                    }
                    case 3:
                    {
                        dot += a.Begin + " -> " + a.End + "[label = \" " + a.Symbol + ":" + a.EdgeTimesUsed.ToString() + "\" color = darkolivegreen4];\n";
                        break;
                    }
                    default:
                    {
                        break;
                    }
                }
            }
            dot += "}";
            try
            {
                File.WriteAllText(timeOnStart, dot);
            }
            catch (Exception)
            {
                File.WriteAllText(timeOnStart, dot);
            }

            var timeOnEnd = DateTime.Now.ToString("yyyyMMddHHmmssffff");
            timeOnEnd += ".gif";

            var proccess = new Process();
            proccess.StartInfo.FileName = "CMD.exe";
            proccess.StartInfo.Arguments = "/c Graphviz\\dot.exe -Tgif -Gdpi=368 "+ timeOnStart + " -o " + timeOnEnd;
            proccess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proccess.Start();
            proccess.WaitForExit();
            try
            {
                File.Delete(timeOnStart);
            }
            catch (Exception)
            {
                File.Delete(timeOnStart);
            }
            return timeOnEnd;
        }
        /// <summary>
        /// Matriz AFD
        /// </summary>
        public List<List<string>> toTransformedOutputMatrix()
        {
            var matrix = new List<List<string>>();

            for (int i = 0; i < Q.Count() + 1; i++)
            {
                matrix.Add(new List<string>());

                for (int j = 0; j < F.Count() + 2; j++)
                {

                    if (i == 0 && j == 0)
                    {
                        matrix[i].Add("Estados");
                    }
                    else if (i == 0 && j == F.Count() + 1)
                    {
                        matrix[i].Add("Comp");
                    }
                    else if (i == 0)
                    {
                        matrix[i].Add(F[j - 1]);
                    }
                    else if (j == 0)
                    {
                        matrix[i].Add("*" + Q[i - 1]);
                    }

                    else if (j == F.Count() + 1)
                    {
                        matrix[i].Add(Comps[i - 1]);
                    }
                    else
                    {
                        matrix[i].Add(Find(Q[i - 1], F[j - 1]));
                    }
                }
            }
            return matrix;
        }
        public string getTransformedData()
        {
            return "Q={" + string.Join(",", Q) + "}\n" +
                "F={" + string.Join(",", F) + "}\n" +
                "i=" + I + "\n" +
                "A={" + string.Join(",", A) + "}";
        }
        /// <summary>
        /// Validación
        /// </summary>
        public bool validateInput(string input)
        {
            foreach (var a in W)
            {
                a.EdgeColorStatus = -1;
                a.EdgeTimesUsed = 0;
            }

            var splitted = new Queue<string>();
            foreach (var a in input)
            {
                splitted.Enqueue(a.ToString());
            }
            var cleaned = splitted.ToList();
            cleaned.Sort();
            var tempArray = cleaned.Distinct().ToArray();
            if (tempArray.Except(F).Any())
            {
                CurrentNodeOnGraph = string.Empty;
                return false;
            }

            CurrentNodeOnGraph = I;
            if (A.Contains(I) && input.Equals(string.Empty))
            {
                return true;
            }

            var result = false;
            var aux = I;
            var path = new List<int>();
            while (splitted.Count > 0)
            {
                var temp = aux;
                var carat = splitted.Dequeue();
                for (int i = 0; i < W.Count(); i++)
                {
                    if (W[i].Find(temp, carat))
                    {
                        aux = W[i].End;
                        path.Add(i);
                        CurrentNodeOnGraph = aux;
                        W[i].EdgeTimesUsed++;
                        break;
                    }
                    aux = string.Empty;
                }
                if (string.IsNullOrEmpty(aux))
                {
                    result = false;
                }
                else if (A.Contains(aux))
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            foreach (var a in path)
            {
                var temp = result ? 1 : 0;
                if (temp == W[a].EdgeColorStatus)
                {
                    W[a].EdgeColorStatus = temp + 2;
                }
                else if (W[a].EdgeColorStatus == -1)
                {
                    W[a].EdgeColorStatus = temp;
                }
            }
            return result;
        }
        public FiveTuple Transform()
        {
            var newComps = new List<List<string>>();
            var newState = new List<string>();
            newState.Add(I);
            var aux = I;
            while (true) {
                aux = Find(aux, LAMBDA);
                if(string.IsNullOrEmpty(aux))
                {
                    break;
                }
                else
                {
                    newState.Add(aux);
                }
            }
            var newLambdasInState = new List<int>();
            newState.Sort();
            var newQ = new List<string>();
            var newStateCharacter = ((char)ASCII_A).ToString();
            newQ.Add(newStateCharacter);
            newComps.Add(newState);
            var newA = new List<string>();
            if (A.Intersect(newState).Any())
            {
                newA.Add(newStateCharacter);
            }
            var newW = new List<Transition>();
            for (int k = 0; k < newComps.Count; k++)
            {
                foreach (string i in F)
                {
                    if (i == LAMBDA)
                    {
                        continue;
                    }
                    var temp = new List<string>();
                    foreach (string j in newComps[k])
                    {
                        aux = Find(j, i);
                        if (!string.IsNullOrEmpty(aux))
                        {
                            temp.Add(aux);
                        }
                        while (true)
                        {
                            aux = Find(aux, LAMBDA);
                            if (string.IsNullOrEmpty(aux))
                            {
                                break;
                            }
                            else
                            {
                                temp.Add(aux);
                            }
                        }
                    }
                    temp.Sort();
                    temp = temp.Distinct().ToList();
                    if (temp.Count == 0)
                    {
                        temp.Add("0");
                    }
                    var start = string.Join(",", newComps[k].ToArray());
                    var character = i;
                    var end = string.Join(",", temp.ToArray());
                    newW.Add(new Transition(start, character, end));
                    var exists = false;
                    foreach (var a in newComps)
                    {
                        exists = Utils.EqualsAll(temp, a);
                        if(exists)
                        {
                            break;
                        }
                    }
                    if (!exists)
                    {
                        newComps.Add(temp);
                        newStateCharacter = ((char)(newQ.Last().ToCharArray()[0] + 1)).ToString();
                        if (A.Intersect(temp).Any())
                        {
                            newA.Add(newStateCharacter);
                        }
                        newQ.Add(newStateCharacter);
                    }
                }
            }
            var transformedFiveTuple = new FiveTuple();
            transformedFiveTuple.Q = newQ.ToArray();
            var newF = new List<string>();
            foreach(var a in F)
            {
                if (a == LAMBDA)
                {
                    continue;
                }
                newF.Add(a);
            }
            transformedFiveTuple.F = newF.ToArray();
            List<string> strComps = new List<string>();
            for (int i = 0; i < newComps.Count; i++)
            {
                strComps.Add(string.Join(",", newComps[i].ToArray()));
            }
            foreach (var a in newW)
            {
                for(int i = 0; i < strComps.Count; i++)
                {
                    if(a.Begin.Equals(strComps[i]))
                    {
                        a.Begin = newQ[i];
                    }
                    if (a.End.Equals(strComps[i]))
                    {
                        a.End = newQ[i];
                    }
                }
            }
            transformedFiveTuple.W = newW.ToArray();
            transformedFiveTuple.I = newQ.First();
            transformedFiveTuple.A = newA.ToArray();
            transformedFiveTuple.Comps = strComps.ToArray();
            return transformedFiveTuple;
        }
        class Transition
        {
            public string Begin { get; set; }
            public string Symbol { get; set; }
            public string End { get; set; }
            public int EdgeColorStatus { get; set; } = -1;
            public int NodeStatus { get; set; } = 0;
            public int EdgeTimesUsed { get; set; } = 0;
            public Transition() { }
            public Transition(string begin, string symbol, string end) {
                Begin = begin;
                Symbol = symbol;
                End = end;
            }
            public bool Find(string begin, string symbol)
            {
                return (Begin == begin && Symbol == symbol);
            }
        }
    }
}
