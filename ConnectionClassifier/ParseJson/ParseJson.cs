using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using ConnectionClassifier.GeometryCalculations;
using Newtonsoft.Json.Linq;
using Tekla.Structures.Geometry3d;
using System.Data;

namespace ConnectionClassifier.ParseJson
{
    internal class ParseJson
    {
        public static dynamic ReadJsonFile(string jsonFileIn)
        {
            dynamic jsonFile = JsonConvert.DeserializeObject(File.ReadAllText(jsonFileIn));
            return jsonFile;            
        }


        public void ClassifyConnection(ConnectionObject connectionObject, dynamic json, dynamic jsonLists, int tolerance)
        {
            foreach (var child in json)
            {
                bool criteriaCheck = ConnectionFulfillsCriteria(connectionObject, child, jsonLists, tolerance);
                if (criteriaCheck)
                {
                    var subclass = child.Subclass;

                    if (subclass != null)
                    {

                        if (subclass.Count > 0)
                        {
                            ClassifyConnection(connectionObject, subclass, jsonLists, tolerance);
                        }
                    }
                    break;
                }
            }
        }

        public bool ConnectionFulfillsCriteria(ConnectionObject connectionObject, dynamic json, dynamic jsonLists, int tolerance)
        {
            string name = json.Name;
            string order = json.Order;
            var decision = json.Decision;

            OrderParts(connectionObject, order, jsonLists);

            if(decision.Value == "Null")
            {
                connectionObject.ConnetionType += "|" + name;
                return true;
            }
            

            bool decisionCheck = DecisionCheck(connectionObject, decision, jsonLists, tolerance);
            if (decisionCheck)
            {
                connectionObject.ConnetionType += "|" + name;
                return true;
            }

            return false;
        }
        
        public bool OrderParts(ConnectionObject connection, string order, dynamic jsonLists)
        {
            switch(order)
            {
                case "FirstIsHCS":
                    FirstIsHCS(connection, jsonLists);
                    return true;
                case "Unspecified":
                    return true;
                default:
                    throw new Exception($"No such order method: {order}");
            }
        }

        public void FirstIsHCS(ConnectionObject connection, dynamic jsonLists)
        {
            PartObject part2 = connection.Part2;

            string profile = part2.Profile;

            var jsonListValues = jsonLists["hcs_profiles"];

            foreach (var listValue in jsonListValues)
            {
                string value = listValue.ToString();
                if (value.Equals(profile))
                {
                    var tempPart = connection.Part2;
                    connection.Part2 = connection.Part1;
                    connection.Part1 = tempPart;

                    var tempPlane = connection.Plane2Parameters;
                    connection.Plane2Parameters = connection.Plane1Parameters;
                    connection.Plane1Parameters = tempPlane;

                    break;
                }
            }
        }

        public bool DecisionCheck(ConnectionObject connectionObject, dynamic decision, dynamic jsonLists, int tolerance)
        {
            string condition = decision.Condition;
            var criteria = decision.Criteria;
            int fullfilledCriteria = 0;
            int criteriaCount = criteria.Count;
            int orXCount = 0;

            if (decision.ContainsKey("Number"))
            {
                orXCount = decision.Number;
            }


            foreach(var child in criteria)
            {
                bool criteriaCheck = CriteriaCheck(connectionObject, child, jsonLists, tolerance);
                if (criteriaCheck)
                {
                    fullfilledCriteria++;
                }
            }
            switch (condition)
            {
                case "OR":
                    if(fullfilledCriteria > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "AND":
                    if(fullfilledCriteria == criteriaCount)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "ORX":
                    if(fullfilledCriteria >= orXCount)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                default:
                    throw new Exception($"No condition: {condition}");
            }
        }

        public bool CriteriaCheck(ConnectionObject connectionObject, dynamic criteria, dynamic jsonLists, int tolerance)
        {
            string rule = criteria.Rule;
            bool criteriaCheck;

            switch(rule)
            {
                case "PropertyInList":
                    criteriaCheck = PropertyInList(connectionObject, criteria, jsonLists);
                    return criteriaCheck;
                case "ValueNumericCheck":
                    criteriaCheck = ValueNumericCheck(connectionObject, criteria, tolerance);
                    return criteriaCheck;
                case "TwoPropertyCheck":
                    criteriaCheck = TwoPropertyCheck(connectionObject, criteria, tolerance);
                    return criteriaCheck;
                case "PropertySet":
                    criteriaCheck = PropertySet(connectionObject, criteria, tolerance);
                    return criteriaCheck;
                case "ValueStringCheck":
                    criteriaCheck = ValueStringCheck(connectionObject, criteria, tolerance);
                    return criteriaCheck;
                default:
                    throw new Exception($"No Criteria Rule: {rule}");                    
            }
        }

        public dynamic GetPart(string part, ConnectionObject connectionObject)
        {
            switch (part)
            {
                case "Part1":
                    return connectionObject.Part1;
                case "Part2":
                    return connectionObject.Part2;
                case "Plane1":
                    return connectionObject.Plane1Parameters;
                case "Plane2":
                    return connectionObject.Plane2Parameters;
                case "ConnectionAngles":
                    return connectionObject.ConnectionAngles;
                default:
                    throw new Exception($"No Part Rule: {part}");
            }
        }

        public bool PropertyInList(ConnectionObject connectionObject, dynamic criteria, dynamic jsonLists)
        {
            string partString = criteria.Part;
            string property = criteria.Property;
            string jsonListKey = criteria.List;

            var jsonListValues = jsonLists[jsonListKey];

            var part = GetPart(partString, connectionObject);

            dynamic dynamicObj = part;
            var partProperty = dynamicObj.GetType().GetProperty(property)?.GetValue(dynamicObj, null)?.ToString();

            foreach(var listValue in jsonListValues)
            {
                string value = listValue.ToString();
                if (value.Equals(partProperty))
                {
                    return true;
                }
            }
            return false;
        }

        public bool ValueNumericCheck(ConnectionObject connectionObject, dynamic criteria, int tolerance)
        {
            string partString = criteria.Part;
            string property = criteria.Property;
            string condition = criteria.Condition;
            var value = criteria.Value.Value;

            var part = GetPart(partString, connectionObject);
            dynamic dynamicObj = part;
            var partProperty = dynamicObj.GetType().GetProperty(property)?.GetValue(dynamicObj, null)?.ToString();

            if (partProperty == null)
            {
                throw new Exception($"No property found: {property}");
            }

            double partPropertyValue = Math.Round(double.Parse(partProperty),2);
            
            switch (condition)
            {
                case ">":
                    if(partPropertyValue > value)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "=":
                    if(partPropertyValue == value)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "<":
                    if (partPropertyValue < value)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "<=":
                    if(partPropertyValue <= value)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case ">=":
                    if (partPropertyValue >= value)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                default:
                    throw new Exception($"No condition found: {condition}");
            }
        }

        public bool ValueStringCheck(ConnectionObject connectionObject, dynamic criteria, int tolerance)
        {
            string partString = criteria.Part;
            string property = criteria.Property;
            string condition = criteria.Condition;
            var value = criteria.Value.Value;

            var part = GetPart(partString, connectionObject);
            dynamic dynamicObj = part;
            var partProperty = dynamicObj.GetType().GetProperty(property)?.GetValue(dynamicObj, null)?.ToString();

            if (partProperty == null)
            {
                throw new Exception($"No property found: {property}");
            }

            switch (condition)
            {
                case "=":
                    if(partProperty.Equals(value))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                default:
                    throw new Exception($"No condition found: {condition}");
            }


        }

        public bool TwoPropertyCheck(ConnectionObject connectionObject, dynamic criteria, int tolerance)
        {
            string part1String = criteria.Part;
            string part2String = criteria.Part2;
            string property1 = criteria.Property;
            string property2 = criteria.Property2;
            string condition = criteria.Condition;

            var part1 = GetPart(part1String, connectionObject);
            dynamic dynamicObj = part1;
            var part1PropertyString = dynamicObj.GetType().GetProperty(property1)?.GetValue(dynamicObj, null)?.ToString();


            var part2 = GetPart(part2String, connectionObject);
            dynamic dynamicObj2 = part2;
            var part2PropertyString = dynamicObj2.GetType().GetProperty(property2)?.GetValue(dynamicObj2, null)?.ToString();

            if (part1PropertyString == null || part2PropertyString == null)
            {
                throw new Exception($"No property found, Property1: {property1} {part1PropertyString}, Property2: {property2} {part2PropertyString}");
            }

            double part1Property = double.Parse(part1PropertyString);
            double part2Property = double.Parse(part2PropertyString);

            switch (condition)
            {
                case "=":
                    if (part1Property == part2Property)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case "<":
                    if (part1Property < part2Property)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                default:
                    throw new Exception($"No condition found: {condition}");
            }
        }

        public bool PropertySet(ConnectionObject connectionObject, dynamic criteria, int tolerance)
        {
            var parts = criteria.Part;
            var properties = criteria.Property;

            connectionObject.ConnetionType += "|PropertySet";

            for (int i = 0; i<parts.Count; i++)
            {
                

                var partString = parts[i].Value;
                var property = properties[i].Value;

                var part = GetPart(partString, connectionObject);
                dynamic dynamicObj = part;
                var partProperty = dynamicObj.GetType().GetProperty(property)?.GetValue(dynamicObj, null)?.ToString();

                double convertedProperty;
                var isNumeric = double.TryParse(partProperty, out convertedProperty);

                dynamic finalProperty;

                if (isNumeric)
                {
                    finalProperty = Math.Round(convertedProperty, tolerance);
                }
                else
                {
                    finalProperty = partProperty;
                }

                if (finalProperty == null)
                {
                    throw new Exception($"No property found, Property: {property}");
                }

                connectionObject.ConnetionType += $"{partString} {property} {finalProperty}";
            }


            return true;
        }
    }
}
