## How does this directory work?

I'm not sure if this is the right way to do things with Cloudformation, but here is how these templates are designed to work.

The principle is to have a bunch of templates, each implementing a specific area of infrastructure/architecture.

  - They are meant to be applied in an order.
  - They also inform each other of things that have been created.
  - They set SSM Parameter Store parameters for working code.
  - The only common parameter is a string identifying the STAGE, used for parameter access, API creation, naming, and so on.

These four principles are intended to ensure that no manual set-up is required to deploy any of this code.
However, it also means that changes to one particular template further up the tree, may require other templates dependent upon it to be
updated also.  This, to me, is the main argument against this approach.

The question is, can we decrease this dependence by naming convention alone, plus careful placement of goods.
For example, when I create a DynamoDB Table, I can put in to the parameter store various identifying values, such as name, Arn or URL.
In the template that creates the table, I also create various Policies that provide access to the table, and the parameters.
All of these are named, using the STAGE that is passed in to the template.

Now, in, say, a Lambda function, we can refer to the policies by name in the functions Role, and refer to the parameters to find the table itself, 
simply by knowing what the names would be, where to add the STAGE part, and so on.  No dependence between stacks, and if parameter access is
every-time, or periodically checked (for long running functions), the no redeployment of other stacks will be required.
