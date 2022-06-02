﻿// This file was auto-generated by ML.NET Model Builder. 
using Microsoft.ML;
using Microsoft.ML.Data;

namespace TelBotApplication_Filters
{
    public partial class FacebookSpam
    {
        public static ITransformer RetrainPipeline(MLContext context, IDataView trainData)
        {
            IEstimator<ITransformer>? pipeline = BuildPipeline(context);
            ITransformer? model = pipeline.Fit(trainData);

            return model;
        }

        /// <summary>
        /// build the pipeline that is used from model builder. Use this function to retrain model.
        /// </summary>
        /// <param name="mlContext"></param>
        /// <returns></returns>
        public static IEstimator<ITransformer> BuildPipeline(MLContext mlContext)
        {
            // Data process configuration with pipeline data transformations
            EstimatorChain<Microsoft.ML.Transforms.KeyToValueMappingTransformer>? pipeline = mlContext.Transforms.ReplaceMissingValues(new[] { new InputOutputColumnPair(@"profile id", @"profile id"), new InputOutputColumnPair(@"#friends", @"#friends"), new InputOutputColumnPair(@"#following", @"#following"), new InputOutputColumnPair(@"#community", @"#community"), new InputOutputColumnPair(@"age", @"age"), new InputOutputColumnPair(@"#postshared", @"#postshared"), new InputOutputColumnPair(@"#urlshared", @"#urlshared"), new InputOutputColumnPair(@"#photos/videos", @"#photos/videos"), new InputOutputColumnPair(@"fpurls", @"fpurls"), new InputOutputColumnPair(@"fpphotos/videos", @"fpphotos/videos"), new InputOutputColumnPair(@"avgcomment/post", @"avgcomment/post"), new InputOutputColumnPair(@"likes/post", @"likes/post"), new InputOutputColumnPair(@"tags/post", @"tags/post"), new InputOutputColumnPair(@"#tags/post", @"#tags/post") })
                                    .Append(mlContext.Transforms.Concatenate(@"Features", new[] { @"profile id", @"#friends", @"#following", @"#community", @"age", @"#postshared", @"#urlshared", @"#photos/videos", @"fpurls", @"fpphotos/videos", @"avgcomment/post", @"likes/post", @"tags/post", @"#tags/post" }))
                                    .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: @"Label", inputColumnName: @"Label"))
                                    .Append(mlContext.Transforms.Conversion.MapKeyToValue(outputColumnName: @"PredictedLabel", inputColumnName: @"PredictedLabel"));

            return pipeline;
        }
    }
}
