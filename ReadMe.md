## Spider Map Site in AWS.

This is the same as the SpiderMapSite repository - or will be - but instead of a single SQS queue, it'll use an SNS topic and multiple queues.

The idea is to get good decoupled services.

I guess the next step would be build a state machine using step functions.  We'll do this first.