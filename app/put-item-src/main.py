import json
import boto3

s3_client = boto3.client("s3")
ddb_client = boto3.client("dynamodb")

def lambda_handler(event, context):
    print("event:", event)
    for record in event["Records"]:
        bucket_name = record["s3"]["bucket"]["name"]
        object_name = record["s3"]["object"]["key"]

        item = {
            "Name": {"S": bucket_name},
            "Value": {"S": object_name},
        }
        ddb_client.put_item(TableName="S3FilesMetadata", Item=item)

    return {
        "statusCode": 200,
    }
