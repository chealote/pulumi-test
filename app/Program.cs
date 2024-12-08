using System.Text.Json;
using Pulumi.Aws.Lambda;
using System.Collections.Generic;
using System.Linq;
using Pulumi;
using Aws = Pulumi.Aws;

return await Deployment.RunAsync(() =>
{
  var role = new Aws.Iam.Role("role", new()
  {
    AssumeRolePolicy = JsonSerializer.Serialize(new Dictionary<string, object?>
    {
      ["Version"] = "2012-10-17",
      ["Statement"] = new[]
      {
        new Dictionary<string, object?>
        {
          ["Action"] = "sts:AssumeRole",
          ["Effect"] = "Allow",
          ["Principal"] = new Dictionary<string, object?>
          {
            ["Service"] = "lambda.amazonaws.com",
            },
        },
      },
    }),
    ManagedPolicyArns = new[]
    {
      Aws.Iam.ManagedPolicy.AWSLambdaBasicExecutionRole.ToString(),
    },
  });

  var policyString = JsonSerializer.Serialize(new Dictionary<string, object?>
    {
      ["Version"] = "2012-10-17",
      ["Statement"] = new[]
      {
        new Dictionary<string, object?>
        {
          ["Action"] = new[]
          {
            "s3:GetObject",
          },
          ["Effect"] = "Allow",
          ["Resource"] = "*",
          },
      },
    });

  var policy = new Aws.Iam.Policy("GetObjectToAllResourcesInS3", new()
  {
    PolicyDocument = policyString,
  });

  var policyAttachment = new Aws.Iam.RolePolicyAttachment("lambda-policy-attachment", new Aws.Iam.RolePolicyAttachmentArgs
  {
    Role = role.Name,
    PolicyArn = policy.Arn
  });

  var function = new Function("TriggerBucketUpload", new FunctionArgs
  {
    Runtime = "python3.9",
    Code = new FileArchive("src/"),
    Handler = "main.lambda_handler",
    Role = role.Arn
  });

  var b = new Aws.S3.Bucket("CheAloteItsABucket", new()
  {
    Acl = Aws.S3.CannedAcl.Private,
  });

  var allowBucket = new Aws.Lambda.Permission("allow_bucket", new()
  {
    StatementId = "AllowExecutionFromS3Bucket",
    Action = "lambda:InvokeFunction",
    Function = function.Arn,
    Principal = "s3.amazonaws.com",
    SourceArn = b.Arn,
  });

  var bn = new Aws.S3.BucketNotification("ItsABucketNotification", new()
  {
    Bucket = b.Id,
    LambdaFunctions = new[]
    {
      new Aws.S3.Inputs.BucketNotificationLambdaFunctionArgs
      {
        LambdaFunctionArn = function.Arn,
        Events = new[]
        {
          "s3:ObjectCreated:*",
        },
        FilterPrefix = "",
        FilterSuffix = "",
      },
    },
  }, new CustomResourceOptions
  {
    DependsOn =
    {
      allowBucket,
    },
  });


  return new Dictionary<string, object?>
    {
        ["policyIWantToCreate"] = policyString,
    };
});
