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

        [TestMethod]
        public void ConceptSchemeGet()
        {
            var recordId = 11;
            try
            {
                var record = ConceptSchemeManager.Get( recordId );


                record = ConceptSchemeManager.Get( "Paygrade" );

                var rowId = new Guid( "B70C175E-B486-42C3-A647-2D964769C0CA" );
                record = ConceptSchemeManager.Get( rowId );
            }
            catch ( Exception ex )
            {
                Assert.Fail( ex.Message );
            }


        }
    }
}
