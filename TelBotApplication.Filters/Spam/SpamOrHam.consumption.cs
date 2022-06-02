﻿// This file was auto-generated by ML.NET Model Builder. 
using Microsoft.ML;
using Microsoft.ML.Data;
namespace TelBotApplication_Filters
{
    public partial class SpamOrHam
    {
        /// <summary>
        /// model input class for SpamOrHam.
        /// </summary>
        #region model input class
        public class ModelInput
        {
            [ColumnName(@"col0")]
            public float Col0 { get; set; }

            [ColumnName(@"col1")]
            public string Col1 { get; set; }

        }

        #endregion

        /// <summary>
        /// model output class for SpamOrHam.
        /// </summary>
        #region model output class
        public class ModelOutput
        {
            [ColumnName(@"col0")]
            public uint Col0 { get; set; }

            [ColumnName(@"col1")]
            public float[] Col1 { get; set; }

            [ColumnName(@"Features")]
            public float[] Features { get; set; }

            [ColumnName(@"PredictedLabel")]
            public float PredictedLabel { get; set; }

            [ColumnName(@"Score")]
            public float[] Score { get; set; }

        }

        #endregion

        private static readonly string MLNetModelPath = Path.Combine(@"E:\Projects\TelBotApplication\TelBotApplication.Filters\Spam\SpamOrHam.zip");

        public static readonly Lazy<PredictionEngine<ModelInput, ModelOutput>> PredictEngine = new Lazy<PredictionEngine<ModelInput, ModelOutput>>(() => CreatePredictEngine(), true);

        /// <summary>
        /// Use this method to predict on <see cref="ModelInput"/>.
        /// </summary>
        /// <param name="input">model input.</param>
        /// <returns><seealso cref=" ModelOutput"/></returns>
        public static ModelOutput Predict(ModelInput input)
        {
            PredictionEngine<ModelInput, ModelOutput>? predEngine = PredictEngine.Value;
            return predEngine.Predict(input);
        }

        private static PredictionEngine<ModelInput, ModelOutput> CreatePredictEngine()
        {
            MLContext? mlContext = new MLContext();
            ITransformer mlModel = mlContext.Model.Load(MLNetModelPath, out DataViewSchema _);
            return mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);
        }
    }
}