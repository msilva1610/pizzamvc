﻿using System;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using Pizza.Contracts;
using Pizza.Contracts.Operations.Requests.Configuration;
using Pizza.Framework.Operations.InternalUtils.RuntimeMetadata.Types;
using Pizza.Persistence;

namespace Pizza.Framework.Operations.InternalUtils
{
    internal static class PagedQueryOverExtensions
    {
        public static QueryOver<TPersistenceModel, TPersistenceModel> AddAlliasesForAllSubpropertiesInViewModel<TPersistenceModel>(
                this QueryOver<TPersistenceModel, TPersistenceModel> query, PersistenceModelPropertiesDescription modelDescription)
            where TPersistenceModel : IPersistenceModel
        {
            foreach (var joinedModel in modelDescription.JoinedModels)
            {
                query.UnderlyingCriteria.CreateAlias(joinedModel.Name, joinedModel.Name, JoinType.InnerJoin);
            }

            return query;
        }

        public static QueryOver<TPersistenceModel, TPersistenceModel> ApplyFilter<TPersistenceModel, TGridModel>(
                this QueryOver<TPersistenceModel, TPersistenceModel> query,
                FilterConfiguration<TGridModel> filterConfiguration,
                ViewModelToPersistenceModelPropertyNamesMaps viewModelToPersistenceModelMap)
            where TPersistenceModel : IPersistenceModel
            where TGridModel : IGridModelBase
        {
            foreach (var condition in filterConfiguration.Conditions)
            {
                if (!viewModelToPersistenceModelMap.AllProperties.ContainsKey(condition.PropertyName))
                {
                    throw new ApplicationException("Property used to filter records can not be found in ViewModelToPersistenceModelMap. Grid metamodel is probably broken.");
                }

                string conditionPropertyName = viewModelToPersistenceModelMap.AllProperties[condition.PropertyName];

                switch (condition.Operator)
                {
                    case FilterOperator.Select:
                        query.UnderlyingCriteria.Add(Restrictions.Eq(conditionPropertyName, condition.Value));
                        break;

                    case FilterOperator.Like:
                        query.UnderlyingCriteria.Add(Restrictions.Like(conditionPropertyName, $"%{condition.Value}%"));
                        break;

                    case FilterOperator.DateEquals:
                        var initDate = (DateTime)condition.Value;
                        var endDate = initDate.AddDays(1).AddSeconds(-1);
                        query.UnderlyingCriteria.Add(Restrictions.Between(conditionPropertyName, initDate, endDate));
                        break;
                }
            }

            return query;
        }

        public static QueryOver<TPersistenceModel, TPersistenceModel> ApplyOrder<TPersistenceModel>(
                this QueryOver<TPersistenceModel, TPersistenceModel> query,
                SortConfiguration sortSettings, ViewModelToPersistenceModelPropertyNamesMaps viewModelToPersistenceModelMap)
            where TPersistenceModel : IPersistenceModel
        {
            if (!viewModelToPersistenceModelMap.AllProperties.ContainsKey(sortSettings.PropertyName))
            {
                throw new ApplicationException("Property used to sort can not be found in ViewModelToPersistenceModelMap. Grid metamodel is probably broken.");
            }

            bool ascending = sortSettings.Mode == SortMode.Ascending;
            string sortPropertyName = viewModelToPersistenceModelMap.AllProperties[sortSettings.PropertyName];
            query.UnderlyingCriteria.AddOrder(new Order(sortPropertyName, ascending));

            return query;
        }

        public static ICriteria ProjectToViewModel<TPersistenceModel, TGridModel>(
                this IQueryOver<TPersistenceModel, TPersistenceModel> query, ProjectionList projectionsList)
        {
            query.UnderlyingCriteria
                .SetProjection(projectionsList)
                .SetResultTransformer(Transformers.AliasToBean<TGridModel>());

            return query.UnderlyingCriteria;
        }
    }
}