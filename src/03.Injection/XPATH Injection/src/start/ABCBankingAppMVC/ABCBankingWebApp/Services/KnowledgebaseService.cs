using ABCBankingWebApp.Models;
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
            
            var matchedNodes = XmlDoc.SelectNodes("//knowledge[tags[contains(.,'" + input + "')] and sensitivity/text()=\'Public\']");

            foreach (XmlNode node in matchedNodes)
            {
                 searchResult.Add(new Knowledge() {Topic = node.SelectSingleNode("topic").InnerText,Description = node.SelectSingleNode("description").InnerText});                
            }

            return searchResult;
        }
    }

    public interface IKnowledgebaseService
    {
        List<Knowledge> Search(string input);
    }
}
