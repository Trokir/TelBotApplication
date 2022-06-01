
using TelBotApplication_Filters;

namespace TelBotApplication.Filters
{
    public class FludFilter : IFludFilter
    {

       
        public bool CheckIsSpam(string value)
        {
            //Load sample data
            var sampleData = new Spamers.ModelInput()
            {
                Col1 = value
            };

            //Load model and predict output
            var result = Spamers.Predict(sampleData).PredictedLabel;
            return (result.Equals("ham")) ? false : true;
        }



    }
}
