using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using Models.Schema;
using Factories;


namespace AppTestProject
{
    [TestClass]
    public class DataTesting
    {

        [TestMethod]
        public void GetAllTesting()
        {
            var existingRatings = RatingManager.GetAll();

            var billetTitle = Factories.JobManager.GetAll();
            var course = Factories.CourseManager.GetAll();
            var organization = Factories.OrganizationManager.GetAll();
            var referenceResource = Factories.ReferenceResourceManager.GetAll();
            var workRole = Factories.WorkRoleManager.GetAll();
            //training task - really all?
            var trainingTask = Factories.TrainingTaskManager.GetAll();

                int totalRows = 0;
            var ratingTask = Factories.RatingTaskManager.GetAllForRating( "q", true, ref totalRows );
        }
      
        [TestMethod]
        public void GetAllConcepts()
        {
            var payGradeTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_Pay_Grade ).Concepts;
            var trainingGapTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_TrainingGap ).Concepts;
            var applicabilityTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_TaskApplicability ).Concepts;
            var sourceTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_ReferenceResource ).Concepts;
            var courseTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_CourseType ).Concepts;
            var assessmentMethodTypeConcepts = Factories.ConceptSchemeManager.GetbyShortUri( Factories.ConceptSchemeManager.ConceptScheme_CurrentAssessmentApproach ).Concepts;
        }
        #region RatingTask
        [TestMethod]
        public void RatingTaskGet()
        {
            var recordId = 1;
            try
            {
                var record = RatingTaskManager.Get( recordId, true );
                record = RatingTaskManager.Get( 10, true );
            } catch ( Exception ex )
            {
                Assert.Fail( ex.Message );
            }


        }
        [TestMethod]
        public void RatingTaskGetAll()
        {
            var totalRows = 0;
            try
            {
                var results = RatingTaskManager.GetAllForRating( "qm", true, ref totalRows );

                results = RatingTaskManager.GetAll( "abf", false, 1, 200, ref totalRows );
            }
            catch ( Exception ex )
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
               var results = RatingTaskManager.RMTLSearch( filter, orderBy, pageNumber, pageSize, userId, ref pTotalRows );
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
        #endregion

        #region Rating
        [TestMethod]
        public void RatingGetAll()
        {
            try
            {
                var list = RatingManager.GetAll();
                foreach ( var item in list )
                {

                }
            }
            catch ( Exception ex )
            {
                Assert.Fail( ex.Message );
            }
        }
        #endregion

        #region Source
        [TestMethod]
        public void ReferenceResourceGet()
        {
            var recordId = 1;
            try
            {
                var record = ReferenceResourceManager.Get( recordId );
                record = ReferenceResourceManager.Get( 119 );
            }
            catch ( Exception ex )
            {
                Assert.Fail( ex.Message );
            }

            //
            //SourceGetAll();
        }
        [TestMethod]
        public void ReferenceResourceGetAll()
        {
            try
            {
                var list = ReferenceResourceManager.GetAll();
                foreach ( var item in list )
                {

                }
            }
            catch ( Exception ex )
            {
                Assert.Fail( ex.Message );
            }
        }
        #endregion
        #region Concept Schemes
        [TestMethod]
        public void ConceptSchemeGet()
        {
            var recordId = 11;
            try
            {
                var record = ConceptSchemeManager.Get( recordId );


                record = ConceptSchemeManager.GetByName( "Paygrade" );

                var rowId = new Guid( "B70C175E-B486-42C3-A647-2D964769C0CA" );
                record = ConceptSchemeManager.Get( rowId );
            }
            catch ( Exception ex )
            {
                Assert.Fail( ex.Message );
            }


        }

        [TestMethod]
        public void ConceptSchemeGetAll()
        {
            try
            {
                var list = ConceptSchemeManager.GetAll();
                foreach( var item in list )
                {

                }
            }
            catch ( Exception ex )
            {
                Assert.Fail( ex.Message );
            }
        }

        #endregion
    }
}
