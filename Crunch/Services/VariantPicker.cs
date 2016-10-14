using Crunch.Attributes;
using System;
using Crunch.Models.Experiments;

namespace Crunch.Services
{
 public interface IVariantPicker
{
    string SelectVariant(TestConfiguration config);
}

[AutoRegister]
public class VariantPicker : IVariantPicker {
    public string SelectVariant(TestConfiguration config) {
        var rand = new Random();
        var choice = rand.Next(1, 100);
        float compoundingCount = 0;

        foreach(var variant in config.Variants) {
            var summedInfluence = (variant.Influence * 100) + compoundingCount;
            if(choice > compoundingCount && choice <= summedInfluence) {
                return variant.Name;
            }

            compoundingCount = summedInfluence;
        }

        throw new Exception("Unable to determine variant");
    }
}

}