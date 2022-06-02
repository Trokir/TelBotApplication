﻿// This file was auto-generated by ML.NET Model Builder. 
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;

namespace TelBotApplication_Filters
{
    public partial class SpamOrHam
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
            EstimatorChain<Microsoft.ML.Transforms.KeyToValueMappingTransformer>? pipeline = mlContext.Transforms.Text.FeaturizeText(inputColumnName: @"col1", outputColumnName: @"col1")
                                    .Append(mlContext.Transforms.Concatenate(@"Features", new[] { @"col1" }))
                                    .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: @"col0", inputColumnName: @"col0"))
                                    .Append(mlContext.Transforms.NormalizeMinMax(@"Features", @"Features"))
                                    .Append(mlContext.MulticlassClassification.Trainers.OneVersusAll(binaryEstimator: mlContext.BinaryClassification.Trainers.LbfgsLogisticRegression(new LbfgsLogisticRegressionBinaryTrainer.Options() { L1Regularization = 0.03515041F, L2Regularization = 1.762164F, LabelColumnName = @"col0", FeatureColumnName = @"Features" }), labelColumnName: @"col0"))
                                    .Append(mlContext.Transforms.Conversion.MapKeyToValue(outputColumnName: @"PredictedLabel", inputColumnName: @"PredictedLabel"));

            return pipeline;
        }
    }
}
