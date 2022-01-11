using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;

using Factories;


namespace AppTestProject
{
    [TestClass]
    public class DataTesting
    {
        [TestMethod]
        public void RatingTaskGet()
        {
            var recordId = 1;
            try
            {
                var record = RatingTaskManager.Get( recordId );
                record = RatingTaskManager.Get( 10 );
            } catch ( Exception ex )
            {
                Assert.Fail( ex.Message );
            }


        }
    }
}
