using Pulumi;
using System.Collections.Generic;
using Pulumi.Aws.Iam;
using Pulumi.Aws.Lambda;
using System.Linq;
using System.Text.Json;
using Aws = Pulumi.Aws;
using AwsApiGateway = Pulumi.AwsApiGateway;

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

    var lambda = new Function("test", new FunctionArgs
    {
        Runtime = "python3.9",
        Code = new FileArchive("src/"),
        Handler = "main.lambda_handler",
        Role = role.Arn
    });

    var api = new AwsApiGateway.RestAPI("api", new()
    {
        Routes =
        {
            new AwsApiGateway.Inputs.RouteArgs
            {
                Path = "/",
                Method = AwsApiGateway.Method.GET,
                EventHandler = lambda,
            },
        },
    });

    // how to get the name of the api here?
    var apiLog = new Aws.CloudWatch.LogGroup("dev", new()
    {
        Name = "apigw-dev",
        RetentionInDays = 1,
    });

    var deploy = new Aws.ApiGateway.Deployment("dev", new()
    {
        RestApi = api.Api.Apply(api => api.Id),
    });

    var devStage = new Aws.ApiGateway.Stage("dev", new()
    {
        Deployment = deploy.Id,
        RestApi = api.Api.Apply(api => api.Id),
        StageName = "dev",
        Variables = new Dictionary<string,string>
        {
            {"STAGE", "dev"}
        },
    });

    var prodStage = new Aws.ApiGateway.Stage("prod", new()
    {
        Deployment = deploy.Id,
        RestApi = api.Api.Apply(api => api.Id),
        StageName = "prod",
        Variables = new Dictionary<string,string>
        {
            {"STAGE", "prod"}
        },
    });

    var permission = new Aws.Lambda.Permission("allowApiGateway", new ()
    {
        StatementId = "AllowExecutionFromApiGateway",
        Action = "lambda:InvokeFunction",
        Function = lambda.Name,
        Principal = "apigateway.amazonaws.com",
    });

    return new Dictionary<string, object?>
    {
        ["devUrl"] = devStage.InvokeUrl,
        ["prodUrl"] = prodStage.InvokeUrl,
    };
});

/*
    var someTable = new Aws.DynamoDB.Table("AnotherTable", new()
    {
        Name = "AnotherTable",
        BillingMode = "PROVISIONED",
        ReadCapacity = 1,
        WriteCapacity = 1,
        HashKey = "UserId",
        RangeKey = "GameTitle",
        Attributes = new[]
        {
            new Aws.DynamoDB.Inputs.TableAttributeArgs
            {
                Name = "UserId",
                Type = "S",
            },
            new Aws.DynamoDB.Inputs.TableAttributeArgs
            {
                Name = "GameTitle",
                Type = "S",
            },
            new Aws.DynamoDB.Inputs.TableAttributeArgs
            {
                Name = "TopScore",
                Type = "N",
            },
        },
        Ttl = new Aws.DynamoDB.Inputs.TableTtlArgs
        {
            AttributeName = "TimeToExist",
            Enabled = true,
        },
        LocalSecondaryIndexes = new[]
        {
            new Aws.DynamoDB.Inputs.TableLocalSecondaryIndexArgs
            {
                Name = "TopScore",
                ProjectionType = "KEYS_ONLY",
                RangeKey = "TopScore",
            },
        },
        Tags =
        {
            { "Name", "someTable" },
            { "Environment", "dev" },
        },
    });
    var ddbPolicy = new RolePolicy("customDynamoDbAccess", new RolePolicyArgs
    {
        Role = lambdaRole.Id,
        Policy =
            @"{
            ""Version"": ""2012-10-17"",
            ""Statement"": [{
                ""Effect"": ""Allow"",
                ""Action"": [
                    ""dynamodb:PutItem"",
                    ""dynamodb:GetItem""
                ],
                ""Resource"": ""arn:aws:dynamodb:*:*:*""
            }]
        }"
    });

*/

