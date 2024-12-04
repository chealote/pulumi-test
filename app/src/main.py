import json
import boto3

ddb = boto3.client("dynamodb")

def lambda_handler(event, context):
    return {
        "statusCode": 200,
        "body": f"Hello world! How to get the stage name?"
    }
