## SQL Injection
## Using ADO.NET

1. Show the filter functionality
2. Demonstrate sql injection by using the following search param

    `sql 
%'; CREATE TABLE User1(Name VARCHAR(20)); select * from sys.tables where 1 =1 or 1='
`

3. Page will execute correctly, but it will create a table in the DB. Show that from the object explorer

## Using EF

1. Show the index method, FromSqlRaw method statement is built using sql statements


# Fixing Issues

## EF

1. Remove the usage of `FromSqlRaw` method to `FromSqlInterpolated` in the action method `FundTransfersAsync` in the file `Controllers\HomeController.cs`

```c#
fundtransfer = _context.FundTransfer.FromSqlInterpolated($"Select * from FundTransfer Where Note Like {SearchString}");
```

The interpolated parameter, `SearchString`, will then be converted into a `DbParameter` object,
making the code safe from SQL injection

2. Parameterization is a proven secure coding practice that will prevent SQL injection. Another way
to rewrite the code in this recipe is to use the DbParameter classes.

```c#
    var searchParam = new SqlParameter("@SearchString", SearchString);

    fundtransfer = _context.FundTransfer.FromSqlRaw("Select * from FundTransfer Where Note Like '%@searchString%' ", searchParam);

    
```

## ADO.NET

You can rewrite the vulnerable ADO.NET code and make it secure by using query parameters.
The `AddWithValue` method from `SqlParametersCollection` of the `SqlCommand` object allows you to add query parameters and safely pass values into the query:

```sql
cmd.Parameters.AddWithValue("@searchparam", search);
```

Changing the search string into a placeholder makes the query parameterized:

```sql
SqlCommand cmd = new SqlCommand("Select * from FundTransfer where Note like @searchparam ", con);
```

A much better approach is to use stored procedures instead of inline statements

```c#
SqlCommand cmd = new SqlCommand(con);
cmd.CommandText = "get_FundTransfers";
cmd.Parameters.AddWithValue("@searchparam", search);
```

## OS Command Injection
ASP.NET Core has a vast set of classes and libraries that enable them to execute OS Commands in the host. if your code is not written securely then this flaw enables to execute shell commands

## Vulnerability

1. Run the application and navigate to the `Backups` page
2. Enter this command injection payload, `backup & calc`, in the `Backup Name` field, and hit the `Create` button
3. The page is now redirected to the Backup Page listing , but along with the calculator app in your machine is also opened
4. If this is not handled, your vulnerable code exposes your host machine to Remote Code Execution(RCE)
5. In the sample code, name for the backup file is provided the user which is not validated. Without validation or sanitization an user can include OS commands which in turn executes unwanted shell command under the web app's identity and authorization

```c#
  public async Task BackupDB(string backupname)
    {
        using (Process p = new Process())
        {
            string source = Environment.CurrentDirectory + "\\OnlineBank.db";
            string destination = Environment.CurrentDirectory + "\\backups\\" + backupname;
            p.StartInfo.Arguments = " /c copy " + source + " " + destination;
            p.StartInfo.FileName = "cmd";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;                
            p.Start();
            await p.WaitForExitAsync();
        }   
    }

```
## Remediation
To remediate this, use  built-in library available in  ASP.NET Core to perform file operations. The above snippet can be rewritten as

```c#
public async Task BackupDB(string backupname)
{
    
    string source = Environment.CurrentDirectory + "\\OnlineBank.db";
    string destination = Environment.CurrentDirectory + "\\backups\\" + backupname;
    
    await FileCopyAsync(source, destination);
    
}

public async Task FileCopyAsync(string sourceFileName, string destinationFileName, int bufferSize = 0x1000, 
            CancellationToken cancellationToken = default(CancellationToken))
{
    using (var sourceFile = File.OpenRead(sourceFileName))
    {
        using (var destinationFile = File.OpenWrite(destinationFileName))
        {
            await sourceFile.CopyToAsync(destinationFile, bufferSize, cancellationToken);
        }
    }
}

```

## XPATH Injection

XPATH is a language construct used for selecting nodes from an XML document, which is somewhat similar to SQL

XPATH formats XML data as tree-structured values

```xml
<catalog> <!-- Document Node -->
   <book id="bk101"> <!-- Element node ,id="bk101" -> attribute node-->
      <author>Gambardella, Matthew</author> <!-- atomic value or item -->
      <title>XML Developer's Guide</title>
      <price>44.95</price>
      <description>An in-depth look at creating applications 
      with XML.</description>
   </book>
</catalog>

```
### Node Relationship

 - `<book>` is the parent node `<author>, <title>, <price> , <description>`

 - `<author>, <title>, <price> , <description>` are **children** of the element `<book>`

 - `<author>, <title>, <price> , <description>` are all **siblings** (same parent)

 - `<catalog>` and `<book>` are **ancestors** of `<author>, <title>, <price> , <description>`

 `<author>, <title>, <price> , <description>` are **descendants** of element `<catalog>`

 ## XPATH Syntax

 - Uses path expressions to select node or node-sets in an XML document

 
| Expression | Description  |
-------------|---------------
| nodename   | Select all child nodes of a named node             |
| /          | Selects from the root node              |
| //         | Select nodes in the current node that match the selection no matter where they are             |
| .          | Select the current node             |
| ..         | Select the parent of the current node              |

**Example**

XML
```xml
<catalog> 
   <book id="bk101"> 
      <author>Gambardella, Matthew</author> 
      <title>XML Developer's Guide</title>
      <price>44.95</price>
      <description>An in-depth look at creating applications 
      with XML.</description>
   </book>
</catalog>

```

XPATH Query : `/catalog/book/title`


| Expression | Result |
-------------|---------
| catalog    | Selects all child nodes of the **catalog** element |
| /catalog   | Selects the root element **catalog** |
| catalog/book | Selects all **book** elements that are children of **catalog**
| //catalog | Selects all **catalog** elements no matter where they are in the document |
| catalog//book | Select all **book** elements that are descendant of the **catalog** element, no matter where they are under the catalog element


## XPATH Predicates

- are ued to find a specific node or node that contains specific value
- predicates are always embedded in  square brackets

| Expression | Result |
-------------|--------|
| /catalog/book[1] | Selects the first **book** element that is the child of the **catalog** element |
| /catalog/book[last()] | Selects the last **book** element this is the child of the **catalog** element |
| /catalog/book[position()<3] | Selects the first two book elements that are children of the **catalog** element |
| //book[@id='bk101'] | Selects all the **book** elements that have an attribute name **id** with value of `bk101`
|

## Vulnerability

Web applications can use XML data store as a mean to store information and records. These data types are in XML format, and one way of navigating
through the nodes of XML is by XPath

As we seen above one can construct XPATH expressions dynamically and sometimes develop can construct XPATH queries with untrusted data

We are making use of the sample xml for help content and uses XPATH query to search help content

```xml
<?xml version="1.0" encoding="utf-8"?>
<knowledgebase>
   <knowledge>
      <topic lang="en">Types of Transfers</topic>
      <description lang="en">
         Make transfers from checking and savings to:
         Checking and savings
         Make transfers from line of credit to:
         Checking and savings
      </description>
      <tags>transfers, transferring funds</tags>
      <sensitivity>Public</sensitivity>
   </knowledge>
   <knowledge>
      <topic lang="en">Expedited Withdrawals</topic>
      <description lang="en">
            Expedited withdrawals are available to our executive account holders.
            You may reach out to Stanley Jobson at stanley.jobson@bank.com
      </description>
      <tags>withdrawals, expedited withdrawals</tags>
      <sensitivity>Confidential</sensitivity>
   </knowledge>
</knowledgebase>

```
And uses the below construct for searching the xml in our service method

```c#
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
```

Here the input search string from the user is not sanitized and one can include special characters or XPATH query which can give you expected results exposing sensitive data.

Take the case of the below input from the user which produces an XPATH query as given below

```
')] or  //node()[('')=('
```

```xpath
//knowledge[tags[contains(.,'')] or  //node()[('')=('')] and sensitivity/text()='Public']
```
Because of the or condition in the query, the and condition is not evaluated and upon execution it will return both sensitive and public data.

## Fixing XPATH Vulnerability

### Input Sanitization

- In this approach we will sanitize the user input so that we will create a whitelist for the characters that are allowed in the search field

```c#
private string Sanitize(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentNullException("input", "input cannot be null");
            }
            HashSet<char> whitelist = new HashSet<char>(@"1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz ");
            return string.Concat(input.Where(i => whitelist.Contains(i))); ;
        }
```

And let's modify the search method in service class to consume this method

```c#

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

```

The `Sanitize` method  will now remove unnecessary and possibly dangerous characters in the input string and the output is then passed into the `sanitizedInput` variable, making the XPath expression safe from exploitation.

## Parameterized Query

- Another approach is to parameterize the XPath query. Here a variable is defined that will serve as a placeholder for an argument

```c#

XPathNavigator nav = XmlDoc.CreateNavigator();
XPathExpression expr = nav.Compile(@"//knowledge[tags[contains(text(),inputParam)] and sensitivity/text()='Public']");
XsltArgumentList varList = new XsltArgumentList();
varList.AddParam("inputParam", string.Empty, input);

CustomContext context = new CustomContext(new NameTable(), varList);
expr.SetContext(context);

var matchedNodes = nav.Select(expr);

foreach (XPathNavigator node in matchedNodes)
{
    searchResult.Add(new Knowledge()
    {
        Topic = node.SelectSingleNode(nav.Compile("topic")).Value, Description = node.SelectSingleNode(nav.Compile("description")).Value
    });
}
```

The XPath expression is modified, and the $inputParam variable is now a
placeholder for the previously concatenated input value
