## Spider Map Site in AWS.

This is the same as the SpiderMapSite repository - or will be - but instead of a single SQS queue, it'll use an SNS topic and multiple queues.
The idea is to get good decoupled services.

So far, we have a Topic, declared in a stack, which exports its name and Arn.
This is in the Topic.template file in Infrastructure.

Then there is a grid ref handling project which is an SQS queue and a lambda function that accepts the queue events.
The queue is subscribed to the topic, and, importantly, the queue has a queue policy which allows the topic to send events to the queue.
This is what allows for the decoupling, I guess - the queue itself is allowing the sns topic to send it messages - not changes
required to the topic itself.

I guess the next step would be build a state machine using step functions.  We'll do this first.