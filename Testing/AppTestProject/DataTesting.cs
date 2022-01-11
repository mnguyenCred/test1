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
        public void RatingTaskSearch()
        {
            var recordId = 1;
            try
            {
                string filter = "";
                string orderBy = "";
                int pageNumber = 1;
                int pageSize = 50;
                int userId = 0;
                int pTotalRows = 0;

                filter = "base.id in (select a.[RatingTaskId] from [RatingTask.HasRating] a inner join Rating b on a.ratingId = b.Id where b.CodedNotation = 'qm' )";
               var results = RatingTaskManager.Search( filter, orderBy, pageNumber, pageSize, userId, ref pTotalRows );
                if (results?.Count > 0)
                {

                }

                results = RatingTaskManager.SearchForRating( "Aviation Boatswain's Mate (Fuels)", orderBy, pageNumber, pageSize, userId, ref pTotalRows );
                if ( results?.Count > 0 )
                {

                }

                results = RatingTaskManager.SearchForRating( "Quartermaster", orderBy, pageNumber, pageSize, userId, ref pTotalRows );
                if ( results?.Count > 0 )
                {

                }
            }
            catch ( Exception ex )
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
