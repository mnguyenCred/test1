﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AppEntity = Models.Schema.ConceptScheme;
using Concept = Models.Schema.Concept;
using DBEntity = Data.Tables.ConceptScheme;

using DataEntities = Data.Tables.NavyRRLEntities;
using ViewContext = Data.Views.ceNavyViewEntities;
using Data.Tables;


using Navy.Utilities;

namespace Factories
{
    public class ConceptSchemeManager
    {
        public static string thisClassName = "ConceptSchemeManager";

        #region Retrieval
        public static AppEntity Get( int id )
        {
            var entity = new AppEntity();
            if ( id < 1 )
                return entity;

            using ( var context = new DataEntities() )
            {
                var item = context.ConceptScheme
                            .SingleOrDefault( s => s.Id == id );

                if ( item != null && item.Id > 0 )
                {
                    MapFromDB( item, entity );
                }
            }

            return entity;
        }

        public static void MapFromDB( DBEntity input, AppEntity output )
        {

            output.Id = input.Id;
            output.Name = input.Name;
            output.Guid = input.RowId;
            output.Description = input.Description;
            //

        }
        public static void MapFromDB( ConceptScheme_Concept input, Concept output )
        {

            if ( input != null && input.Id > 0 )
            {
                output.Id = input.Id;
                output.Guid = input.RowId;
                output.Label = input.Label;
                output.Description = input.Description;
                output.CTID = input.CTID;
            }
            //

        }

        public static Concept MapConcept( ConceptScheme_Concept input )
        {
            var output = new Concept();
            if ( input != null && input.Id > 0 )
            {
                output.Id = input.Id;
                output.Guid = input.RowId;
                output.Label = input.Label;
                output.Description = input.Description;
                output.CTID = input.CTID;
            }

            return output;
        }
        #endregion
    }
}
