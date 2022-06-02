
using TelBotApplication_Filters;

namespace TelBotApplication.Filters
{
    public class FludFilter : IFludFilter
    {


        public bool CheckIsSpam(string value)
        {
            //Load sample data
            Spamers.ModelInput? sampleData = new Spamers.ModelInput()
            {
                Col1 = value
            };

            //Load model and predict output
            string? result = Spamers.Predict(sampleData).PredictedLabel;
            return !result.Equals("ham");
        }

        public bool CheckIsSpamOrHam(string value)
        {
            //Load sample data
            SpamOrHam.ModelInput? sampleData = new SpamOrHam.ModelInput()
            {
                Col1 = value
            };
            _ = SpamOrHam.Predict(sampleData);
            //Load model and predict output
            float result = SpamOrHam.Predict(sampleData).PredictedLabel;
            return result == 1;
        }

        public bool IsFacebookSpam(string value)
        {
            //Load sample data
            FacebookSpam.ModelInput? sampleData = new FacebookSpam.ModelInput()
            {
                Profile_id = 2F,
                __friends = 150F,
                __following = 350F,
                __community = 30F,
                Age = 300F,
                __postshared = 300F,
                __urlshared = 100F,
                __photos_videos = 290F,
                Fpurls = 0.33F,
                Fpphotos_videos = 0.96F,
                Avgcomment_post = 0.5F,
                Likes_post = 1.2F,
                Tags_post = 10F,
                __tags_post = 4F,
            };

            //Load model and predict output
            _ = FacebookSpam.Predict(sampleData);
            return true;
        }



    }
}
