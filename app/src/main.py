import json
import boto3

s3_client = boto3.client("s3")

def lambda_handler(event, context):
    print("event:", event)
    all_metas = []
    for record in event["Records"]:
        bucket_name = record["s3"]["bucket"]["name"]
        object_name = record["s3"]["object"]["key"]
        metadata = s3_client.head_object(Bucket=bucket_name, Key=object_name)
        print(metadata)
        all_metas.append(metadata["Metadata"])
    return {
        "statusCode": 200,
        "body": json.dumps(all_metas)
    }
