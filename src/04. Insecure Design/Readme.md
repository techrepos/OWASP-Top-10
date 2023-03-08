# Insecure Design

Insecure design is a broad category representing different weaknesses, expressed as *missing or ineffective control design.*. An insecure design cannot be fixed by a perfect implementation as by definition, needed security controls were never created to defend against specific attacks

**Scenario #1**: A credential recovery workflow might include “questions and answers,” which is prohibited by NIST 800-63b, the OWASP ASVS, and the OWASP Top 10. Questions and answers cannot be trusted as evidence of identity as more than one person can know the answers, which is why they are prohibited. Such code should be removed and replaced with a more secure design.

**Scenario #2**: A cinema chain allows group booking discounts and has a maximum of fifteen attendees before requiring a deposit. Attackers could threat model this flow and test if they could book six hundred seats and all cinemas at once in a few requests, causing a massive loss of income.

# Secure Product Design

 Ensure that all products meet or exceed the security requirements laid down by the organization as part of the development lifecycle. It establishes secure defaults, minimise the attack surface area, and fail securely to those well-defined and understood defaults.

### Methodology

 It involves mainly two processes

 1. Product Inception
    Occurs when a new product is conceived or an existing product is being re-invented
 2. Product Design
    Is continuous process, performed in an agile way and closely coupled with the coding phase


### Security Principles

 1. Principle of least privilege.

    It states that the users should only be given the minimum amount of access that is needed for executing the job and no more.  It helps to reduce the risk of unauthorized access to sensitive data or systems

 2. Separation of duties

    Is a fundamental principle of internal control in business and organizations
    It ensures that no single individual has control over all aspects of a transaction. It also helps to reduce the risk of fraud and errors

 3. Principle of Defense-in-Depth

    Is a security strategy that involves multiple layers of security controls to protect and an organization's assets. If one layer of security fails, then the other layers will still be able to protect the asset. It typically includes physical security, network security, application security and data security

 4. Principle of Zero Trust
    
    Is a security model that assumes that all users, devices and networks are untrusted and must be verified before access is granted. All requests for access must be authenticated and authorized before access is granted

 5. Principle of Security in the open
    
    Is a concept that empathizes the importance of security in open source software development. This includes using secure coding practices, testing for vulnerabilities, and using secure development tools.  Security-in-the-Open also encourages developers to collaborate with security experts to ensure that their code is secure.


### Secure Coding Basics

1. **Input validation**:

    Verify that all input data is valid and of the expected type, format, and length before processing it. This can help prevent attacks such as SQL injection and buffer overflows.
2. **Error handling**: 

    Handle errors and exceptions in a secure manner, such as by logging them in a secure way and not disclosing sensitive information to an attacker.
3. **Authentication and Authorization**: 

    Implement strong authentication and authorization mechanisms to ensure that only authorized users can access sensitive data and resources.

4. **Cryptography**:
    
    Use cryptographic functions and protocols to protect data in transit and at rest, such as HTTPS and encryption - the expected levels for a given Product Security Level can often be found by reviewing your Golden Path / Paved Road documentation.
5. **Least privilege**: 
    
    Use the principle of the least privilege when writing code, such that the code and the system it runs on are given the minimum access rights necessary to perform their functions.
6. **Secure memory management**: 

    Use high-level languages recommended in your Golden Path / Paved Road documentation or properly manage memory to prevent memory-related vulnerabilities such as buffer overflows and use-after-free.

7. **Avoiding hardcoded secrets**:

    Hardcoded secrets such as passwords and encryption keys should be avoided in the code and should be stored in a secure storage.

8. **Security testing**: 

    Test the software for security vulnerabilities during development and just prior to deployment.

9. **Auditing and reviewing the code**: 

    Regularly audit and review the code for security vulnerabilities, such as by using automated tools or having a third party review the code.

10. **Keeping up-to-date**: 

    Keep the code up-to-date with the latest security best practices and vulnerability fixes to ensure that the software is as secure as possible.


## Securing Configuration

1. Bearing in mind the principle of Least Privilege: Limit the access and permissions of system components and users to the minimum required to perform their tasks.

2. Remembering Defense-in-Depth: Implement multiple layers of security controls to protect against a wide range of threats.

3. Ensuring Secure by Default: Configure systems and software to be secure by default, with minimal manual setup or configuration required.

4. Secure Data: Protect sensitive data, such as personal information and financial data, by encrypting it in transit and at rest. Protecting that data also means ensuring it's correctly backed up and that the data retention is set correctly for the desired Product Security Level.

5. Plan to have the configuration Fail Securely: Design systems to fail in a secure state, rather than exposing vulnerabilities when they malfunction.

6. Always use Secure Communications: Use secure protocols for communication, such as HTTPS, to protect against eavesdropping and tampering.

7. Perform regular updates - or leverage maintained images: Keeping software, docker images and base operating systems up-to-date with the latest security patches is an essential part of maintaining a secure system.

8. Have a practiced Security Incident response plan: Having a plan in place for how to respond to a security incident is essential for minimizing the damage caused by any successful attack and a crucial part of the Product Support Model.



 

