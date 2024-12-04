import json

def lambda_handler(event, context):
    print(event)
    print(context)
    if event is not None and \
       'stageVariables' in event and \
       'STAGE' in event['stageVariables']:
            return {
                "statusCode": 200,
                "body": f"Hello world from {event['stageVariables']['STAGE']}!"
            }
    return {
        "statusCode": 200,
        "body": f"Hello world! No info for stage, sorry..."
    }
