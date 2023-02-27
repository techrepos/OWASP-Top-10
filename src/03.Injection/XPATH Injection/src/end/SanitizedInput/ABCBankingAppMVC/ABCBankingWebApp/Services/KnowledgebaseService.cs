﻿using ABCBankingWebApp.Models;
using System.Xml;

namespace ABCBankingWebApp.Services
{
    public class KnowledgebaseService : IKnowledgebaseService
    {

        private IWebHostEnvironment _env;

        public KnowledgebaseService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public List<Knowledge> Search(string input)
        {
            List<Knowledge> searchResult = new List<Knowledge>();
            var webRoot = _env.WebRootPath;
            var file = System.IO.Path.Combine(webRoot, "Knowledgebase.xml");

            XmlDocument XmlDoc = new XmlDocument();
            XmlDoc.Load(file);

            string sanitizedInput = Sanitize(input);

            var matchedNodes = XmlDoc.SelectNodes("//knowledge[tags[contains(.,'" + sanitizedInput + "')] and sensitivity/text()=\'Public\']");

            foreach (XmlNode node in matchedNodes)
            {
                 searchResult.Add(new Knowledge() {Topic = node.SelectSingleNode("topic").InnerText,Description = node.SelectSingleNode("description").InnerText});                
            }

            return searchResult;
        }


        private string Sanitize(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentNullException("input", "input cannot be null");
            }
            HashSet<char> whitelist = new HashSet<char>(@"1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz ");
            return string.Concat(input.Where(i => whitelist.Contains(i))); ;
        }
    }

    public interface IKnowledgebaseService
    {
        List<Knowledge> Search(string input);
    }
}
