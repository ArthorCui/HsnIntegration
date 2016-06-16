using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.Reflection;

namespace ReplaceTokensLib
{
    public class TokenUtils
    {
        //public enum ReturnCodes
        //{
        //    SUCCESS = 0,
        //    ANSWER_FILE_NOT_FOUND = 1,
        //    TEMPLATE_FILE_NOT_FOUND = 2,
        //    TOKENS_IN_TEMPLATE_FILE_BUT_NOT_IN_ANSWER_FILE = 3,
        //    ERROR = 4
        //}

        public static void ReplaceTokens(string templateFile, string outputFile, Dictionary<string, string> tokens)
        {
            string[] lines = File.ReadAllLines(templateFile);
            for (int i = 0; i < lines.Length; i++)
                foreach (string token in tokens.Keys)
                    lines[i] = lines[i].Replace(token, tokens[token]);
            File.WriteAllLines(outputFile, lines);
        }

        public static void ReplaceTokens(string answerFile, string templateFile, string outputFile, out List<string> tokensNotInAnswerFile)
        {
            Dictionary<string, string> answers = new Dictionary<string, string>();
            Dictionary<string, List<Dictionary<string, string>>> dynamicAnswers = new Dictionary<string, List<Dictionary<string, string>>>();
            List<string> templateKeys = new List<string>();
            XmlDocument docAnswerFile = new XmlDocument();
            Regex regex = new Regex(@"\$\{[^""$]+\}");
            //Regex regex = new Regex(@"\$\{[^\}]+/[^\}]+[^""]+\}");
            Regex dynamicRegex = new Regex(@"\$\{[^/]+[^\}]+[^""]\}");
            Regex dynamicRegexStart = new Regex(@"\$\{DynamicBlock Group="".+""\}");
            Regex dynamicRegexEnd = new Regex(@"\$\{EndDynamicBlock\}");
            tokensNotInAnswerFile = new List<string>();

            // Make sure required files exist
            if (!File.Exists(answerFile))
                throw new Exception(String.Format("Answer file not found: {0}", answerFile));

            if (!File.Exists(templateFile))
                throw new Exception(String.Format("Template file not found: {0}", templateFile));

            // Load answer file values
            docAnswerFile.Load(answerFile);
            foreach (XmlNode group in docAnswerFile.SelectNodes("/AnswerFile/Group"))
            {
                string groupName = group.Attributes["Name"].Value;

                // Determine if group is part of a dynamic group
                XmlNodeList groupList = docAnswerFile.SelectNodes(string.Format("/AnswerFile/Group[@Name='{0}']", groupName));
                if (groupList.Count == 1)
                {
                    foreach (XmlNode key in group.SelectNodes("Key"))
                    {
                        string keyName = key.Attributes["Name"].Value;
                        string value = key.SelectSingleNode("Value").InnerText;
                        answers.Add(string.Format("{0}|{1}", groupName, keyName), value);
                    }
                }
                else
                {
                    // Only process this dynamic answer group if it has not already been processed
                    if (!dynamicAnswers.ContainsKey(groupName))
                    {
                        List<Dictionary<string, string>> dynamicGroups = new List<Dictionary<string, string>>();
                        foreach (XmlNode dynamicGroup in groupList)
                        {
                            Dictionary<string, string> dynamicKeys = new Dictionary<string, string>();
                            foreach (XmlNode key in dynamicGroup.SelectNodes("Key"))
                            {
                                string keyName = key.Attributes["Name"].Value;
                                string value = key.SelectSingleNode("Value").InnerText;
                                dynamicKeys.Add(keyName, value);
                            }
                            dynamicGroups.Add(dynamicKeys);
                        }
                        dynamicAnswers.Add(groupName, dynamicGroups);
                    }
                }
            }

            // Load template file keys
            using (TextReader reader = new StreamReader(templateFile))
            {
                string line;
                bool inDynamicBlock = false;
                string dynamicBlockName = string.Empty;

                while ((line = reader.ReadLine()) != null)
                {
                    if (dynamicRegexStart.IsMatch(line))
                    {
                        inDynamicBlock = true;
                        dynamicBlockName = new Regex(@""".+""").Match(line).Value.Replace("\"", "");
                    }
                    else if (dynamicRegexEnd.IsMatch(line))
                        inDynamicBlock = false;
                    else if (!inDynamicBlock)
                    {
                        foreach (Match match in regex.Matches(line))
                        {
                            string matchline = match.Value;
                            matchline = matchline.Substring(2, matchline.Length - 3);
                            string matchgroup = matchline.Substring(0, matchline.IndexOf('/'));
                            string matchvalue = matchline.Substring(matchline.IndexOf('/') + 1);

                            // Ensure this token is not part of a dynamic group
                            if (dynamicAnswers.ContainsKey(matchgroup))
                                throw new Exception(string.Format("Tokens not contained within a dynamic block cannot refer to dynamic answers! The invalid token is: {0}", match.Value));

                            templateKeys.Add(string.Format("{0}|{1}", matchgroup, matchvalue));
                        }
                    }
                    else
                    {
                        foreach (Match match in regex.Matches(line))
                        {
                            string matchline = match.Value;
                            matchline = matchline.Substring(2, matchline.Length - 3);

                            int slashPos = matchline.IndexOf('/');
                            if (slashPos >= 0)
                            {
                                // Tokens in a dynamic group have the ability to reference different groups
                                string matchgroup = matchline.Substring(0, slashPos);
                                string matchvalue = matchline.Substring(slashPos + 1);

                                // But they can't access tokens in other dynamic groups
                                if (dynamicAnswers.ContainsKey(matchgroup))
                                    throw new Exception(string.Format("Tokens contained within a dynamic block cannot refer to answers in a different dynamic block! The invalid token is: {0}", match.Value));

                                if (!templateKeys.Contains(string.Format("{0}|{1}", matchgroup, matchvalue)))
                                    templateKeys.Add(string.Format("{0}|{1}", matchgroup, matchvalue));
                            }
                            else
                            {
                                // They also have the ability to refer to the current dynamic group
                                string matchgroup = dynamicBlockName;
                                string matchvalue = matchline;

                                if (!templateKeys.Contains(string.Format("{0}|{1}", matchgroup, matchvalue)))
                                    templateKeys.Add(string.Format("{0}|{1}", matchgroup, matchvalue));
                            }
                        }
                    }
                }
            }

            // Check to make sure template file keys exist in answer file
            bool tokensFoundInTemplateFileButNotInAnswerFile = false;
            foreach (string templateKey in templateKeys)
            {
                if (!answers.ContainsKey(templateKey))
                {
                    // Check to see if key is contained within a dynamic group
                    string groupName = templateKey.Split(new char[] { '|' })[0];
                    string keyName = templateKey.Split(new char[] { '|' })[1];
                    if (!dynamicAnswers.ContainsKey(groupName))
                    {
                        tokensFoundInTemplateFileButNotInAnswerFile = true;
                        if (!tokensNotInAnswerFile.Contains(templateKey))
                            tokensNotInAnswerFile.Add(templateKey);
                    }
                    else
                    {
                        Dictionary<string, string> dynamicGroup = dynamicAnswers[groupName][0];
                        if (!dynamicGroup.ContainsKey(keyName))
                        {
                            tokensFoundInTemplateFileButNotInAnswerFile = true;
                            if (!tokensNotInAnswerFile.Contains(templateKey))
                                tokensNotInAnswerFile.Add(templateKey);
                        }
                    }
                }
            }
            if (tokensFoundInTemplateFileButNotInAnswerFile)
            {
                // Tokens are present that are not in the answer file.  Generate an exception message listing all missing tokens.
                StringBuilder message = new StringBuilder();
                message.AppendLine(string.Format("ERROR: The tokens listed below appear to be missing from your {0} file:", answerFile));
                message.AppendLine(string.Empty);
                foreach (string token in tokensNotInAnswerFile)
                {
                    string tokenLine = "${" + token.Substring(0, token.IndexOf("|")) + "/" + token.Substring(token.IndexOf("|") + 1) + "}";
                    message.AppendLine(tokenLine); 
                }

                message.AppendLine(string.Empty);
                message.AppendLine(string.Format("These tokens are needed to process the file {0}.", templateFile));
                throw new Exception(message.ToString());
            }

            // Perform token replacement
            using (TextReader reader = new StreamReader(templateFile))
            using (TextWriter writer = new StreamWriter(outputFile))
            {
                string line, outputline, dynamicLine, dynamicBlockText;
                bool inDynamicBlock = false;
                string dynamicBlockName = string.Empty;

                while ((line = reader.ReadLine()) != null)
                {
                    if (dynamicRegexStart.IsMatch(line))
                    {
                        inDynamicBlock = true;
                        dynamicBlockName = new Regex(@""".+""").Match(line).Value.Replace("\"", "");
                    }
                    else if (dynamicRegexEnd.IsMatch(line))
                        inDynamicBlock = false;
                    if (!inDynamicBlock)
                    {
                        outputline = line;
                        foreach (Match match in regex.Matches(line))
                        {
                            string matchline = match.Value;
                            matchline = matchline.Substring(2, matchline.Length - 3);
                            string matchgroup = matchline.Substring(0, matchline.IndexOf('/'));
                            string matchvalue = matchline.Substring(matchline.IndexOf('/') + 1);
                            string answervalue = answers[string.Format("{0}|{1}", matchgroup, matchvalue)];
                            outputline = outputline.Replace(match.Value, answervalue);
                        }
                        writer.WriteLine(outputline);
                    }
                    else
                    {
                        // Get dynamic block text
                        dynamicBlockText = string.Empty;
                        while ((dynamicLine = reader.ReadLine()) != null)
                        {
                            if (dynamicRegexEnd.IsMatch(dynamicLine))
                            {
                                inDynamicBlock = false;
                                break;
                            }

                            dynamicBlockText += dynamicLine + Environment.NewLine;
                        }

                        List<Dictionary<string, string>> answerList;
                        if (dynamicAnswers.ContainsKey(dynamicBlockName))
                            answerList = dynamicAnswers[dynamicBlockName];
                        else
                        {
                            answerList = new List<Dictionary<string, string>>();
                            answerList.Add(GetAnswersForGroup(answers, dynamicBlockName));
                        }


                        // Loop through dynamic block answers
                        foreach (Dictionary<string, string> dynAnswers in answerList)
                        {
                            string replacedBlockText = dynamicBlockText;
                            foreach (Match match in regex.Matches(dynamicBlockText))
                            {
                                string matchline = match.Value;
                                matchline = matchline.Substring(2, matchline.Length - 3);
                                int slashPos = matchline.IndexOf('/');
                                if (slashPos >= 0)
                                {
                                    // Tokens in a dynamic group have the ability to reference different groups
                                    string matchgroup = matchline.Substring(0, slashPos);
                                    string matchvalue = matchline.Substring(slashPos + 1);
                                    string answervalue = answers[string.Format("{0}|{1}", matchgroup, matchvalue)];
                                    replacedBlockText = replacedBlockText.Replace(match.Value, answervalue);
                                }
                                else
                                {
                                    // They also have the ability to refer to the current dynamic group
                                    string answervalue = dynAnswers[matchline];
                                    replacedBlockText = replacedBlockText.Replace(match.Value, answervalue);
                                }
                            }
                            writer.WriteLine(replacedBlockText);
                        }
                    }
                }
            }
        }

        private static Dictionary<string, string> GetAnswersForGroup(Dictionary<string, string> answers, string groupName)
        {
            Dictionary<string, string> result = new Dictionary<string,string>();
            foreach (string key in answers.Keys)
            {
                string[] parts = key.Split('|');
                if (parts[0] == groupName)
                {
                    result.Add(parts[1], answers[key]);
                }
            }
            return result;
        }


    }

}
