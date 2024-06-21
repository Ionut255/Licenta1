using Sources.Architecture.Interfaces;
using UnityEngine;

namespace Sources.Data
{
    [CreateAssetMenu(fileName = "Manager", menuName = "Data/Create manager", order = 0)]
    public class ManagerData : VisualScriptableObject
    {
        public GeneratorData Generator => GeneratorData;
        public ResourceData Resource => CostResource;
        public double Value => CostValue;
       // public double Value1 => CostValue1;
      //  public double Value2 => CostValue2;
       // public ResourceData Resource1 => CostResource1;
      //  public ResourceData Resource2 => CostResource2;

        [SerializeField] private GeneratorData GeneratorData;
        [SerializeField] private ResourceData CostResource;
        [SerializeField] private double CostValue;
        //[SerializeField] private double CostValue1;
        //[SerializeField] private ResourceData CostResource1;
        //[SerializeField] private double CostValue2;
        //[SerializeField] private ResourceData CostResource2;

    }
}