using PCAT.Common.Models;
using PCAT.Common.Models.Attribute;
using PCAT.Common.Models.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceCatalog.BL
{
    public static class ServiceKeyExtensions
    {
        public static void AddMissingAttributes(this ServiceKey key)
        {
            AddMissingParentAttributes(key);
            AddMissingRelatedAttributes(key);
            AddMissingDefaultAttributes(key);
        }

        public static void AddMissingDefaultAttributes(this ServiceKey key)
        {
            var childConfig = ServiceConfiguration.Get(key.Id, key.EffectiveDate);
            string value;
            foreach (var attribute in childConfig.Attributes)
            {
                if (attribute.Value.Type != AttributeType.List
                    && key.GetAttributeValue(attribute.Value.Name, SearchOptions.ALL_FALSE) == null
                    && !childConfig.IsOptional(attribute.Value.Name, key))
                {
                    value = childConfig.GetDefaultValue(attribute.Value.Name, key);
                    if (value != null)
                        key.AddAttribute(attribute.Value.Name, value);
                }
            }
        }

        public static void AddMissingParentAttributes(this ServiceKey key)
        {
            if (key.ParentServiceKey != null)
            {
                key.ParentServiceKey.AddMissingAttributes();

                var childConfig = ServiceConfiguration.Get(key.Id, key.EffectiveDate);
                var parentConfig = ServiceConfiguration.Get(key.ParentServiceKey.Id, key.ParentServiceKey.EffectiveDate);
                string value;
                foreach (var attribute in childConfig.Attributes)
                {
                    if (attribute.Value.Type == AttributeType.Parent)
                    {
                        value = key.ParentServiceKey.GetAttributeValue(attribute.Value.Name, SearchOptions.ALL_TRUE);
                        if (value == null)
                            value = parentConfig.GetDefaultValue(attribute.Value.Name, key.ParentServiceKey);
                        if (value != null)
                            key.AddAttribute(attribute.Value.Name, value);
                    }
                }
            }
        }

        public static void AddMissingRelatedAttributes(this ServiceKey key)
        {
            if (key.Relationships != null && key.Relationships.Count > 0)
            {
                var childConfig = ServiceConfiguration.Get(key.Id, key.EffectiveDate);
                string value;
                foreach (var attribute in childConfig.Attributes)
                {
                    if (attribute.Value.Type == AttributeType.Related)
                    {
                        value = key.GetAttributeValue(attribute.Value.DefaultValue, SearchOptions.ALL_TRUE);
                        if (value != null)
                            key.AddAttribute(attribute.Value.Name, value);
                    }
                }
            }
        }
    }
}
