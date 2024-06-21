using System.Collections;
using Sources.Architecture.Interfaces;
using Sources.Data;
using UniRx;
using UnityEngine;

namespace Sources.Models
{

    public class Generator : IGenerator
    {
        private double ProductionFunc()
        {
            GenProduc = _baseProduction + Level.Value;

            if (Level.Value < 25)
                Moneygen = (GenProduc + 1) * Level.Value ;
            else if (Level.Value >= 25 && Level.Value < 50)
                Moneygen = ((GenProduc + 5) * Level.Value) * 1.25;
            else if (Level.Value >= 50 && Level.Value < 255)
                Moneygen = ((GenProduc + 25) * Level.Value) * 1.5;
            else if (Level.Value >= 255)
                Moneygen = ((GenProduc + 50) * Level.Value) * 2.55;
            return Moneygen;

        }
        private double UpgradeCost()
        {
            UpgCosHel = _baseUpgradeCost + (Level.Value + 1);

            if (Level.Value < 10)
                UpgCost = _baseUpgradeCost * (Level.Value + 1);
            else if (Level.Value >= 10 && Level.Value < 25)
                UpgCost = ((UpgCosHel + 10) * Level.Value )  * 1.5;
            else if (Level.Value >= 25 && Level.Value < 50 )
                UpgCost = ((UpgCosHel + 50) * Level.Value) * 2;
            else if (Level.Value >= 50 && Level.Value < 125) 
                UpgCost = ((UpgCosHel + 100) * Level.Value) * 2.5;
            else if (Level.Value >= 125 && Level.Value < 255)
                UpgCost = ((UpgCosHel + 200) * Level.Value) * 2.5;
            else if (Level.Value >= 255)
                UpgCost = ((UpgCosHel + 255 * 2.55) * Level.Value) * 2.55;
            return UpgCost;

        }
        private float GenTime()
        {
            if (Level.Value < 10)
                deltime = _baseDelayTime * 1f;
            else if (Level.Value >= 10 && Level.Value < 25)
                deltime = _baseDelayTime / 1.25f;
            else if (Level.Value >= 25 && Level.Value < 50)
                deltime = _baseDelayTime / 1.5f;
            else if (Level.Value >= 50 && Level.Value < 100)
                deltime = _baseDelayTime / 2f;
            else if (Level.Value >= 100 && Level.Value < 255)
                deltime = _baseDelayTime / 4f;
            else if (Level.Value >= 255)
                deltime = 1f / 5f;
            return deltime;
        }
        public double Moneygen;
        public double GenProduc;
        public double UpgCost;
        public double UpgCosHel;
        public float deltime;
        public ReactiveCommand<double> OnEnd { get; private set; }
        public string Name { get; }
        public string Description { get; }
        public Sprite Icon { get; }
        public IResource ProductionResource { get; }
        public IResource CostResource { get; }
        public IResource CostResource1 { get; }
        public IResource CostResource2 { get; }
        public IReadOnlyReactiveProperty<int> Level => _level;
        public double ProductionValue => ProductionFunc();
        

        public double CostValue => UpgradeCost();
        public float DelayTime => GenTime();
        public IReadOnlyReactiveProperty<float> Progress => _progress;

        private readonly double _baseProduction;
        private readonly double _baseUpgradeCost;
        private readonly float _baseDelayTime;
        private readonly ReactiveProperty<float> _progress;
        private readonly ReactiveProperty<int> _level;

        
        
        private Generator(GeneratorData data, IResource productionResource, IResource costResource, int level = 0,
            float progress = 0)
        {
            ProductionResource = productionResource;
            _level = new ReactiveProperty<int>(level);
            Name = data.Name;
            Description = data.Description;
            Icon = data.Icon;
            _baseProduction = data.BaseProduction;
            _baseUpgradeCost = data.BaseCost;
            _baseDelayTime = data.BaseDelay;
            CostResource = costResource;
            _progress = new ReactiveProperty<float>(progress);
            OnEnd = new ReactiveCommand<double>(Progress.Select(p => p == 0f).AsObservable(), true);
            if (_progress.Value != 0f)
            {
                Produce();
            }
        }


        private Generator(IResource resource, double baseCost, double baseProduction, float baseDelay)
        {
            ProductionResource = resource;
            CostResource = resource;
            _baseUpgradeCost = baseCost;
            _baseProduction = baseProduction;
            _baseDelayTime = baseDelay;
            _level = new ReactiveProperty<int>(1);
            _progress = new ReactiveProperty<float>(0);
            OnEnd = new ReactiveCommand<double>(Progress.Select(p => p == 0f).AsObservable(), true);
        }

        public static Generator Load(GeneratorData data, IResource productionResource, IResource costResource)
        {
            var level = PlayerPrefs.GetInt($"Generator Level: {data.Name}", data.IsUnlockedByDefault ? 1 : 0);
            var progress = PlayerPrefs.GetFloat($"Generator progress: {data.Name}", 0f);
            return new Generator(data, productionResource, costResource, level, progress);
        }

        public static Generator CreateMock(IResource resource, double baseCost = 0, double baseProduction = 0,
            float baseDelay = 0f)
        {
            return new Generator(resource, baseCost, baseProduction, baseDelay);
        }

        public void TryProduce()
        {
            if (Progress.Value == 0f)
            {
                Produce();
            }
        }

        public bool CanUpgrade(int levelValue)
        {
            return (CostResource.CurrentValue.Value - CostValue) >= 0f;
        }

        public double GetCost(int levels)
        {
            if (levels == -1)
            {
                return MaxLevelCanBuy() * CostValue;
            }

            return CostValue * levels;
        }

        public bool TryUpgrade(int levelAmount)
        {
            var amount = levelAmount;
            if (levelAmount == -1)
            {
                amount = MaxLevelCanBuy();
            }

            if (CostResource.TrySpend(GetCost(amount)))
            {
                _level.Value += amount;
                return true;
            }

            return false;
        }
        
        public void Save()
        {
            PlayerPrefs.SetInt($"Generator Level: {Name}", Level.Value);
            PlayerPrefs.SetFloat($"Generator progress: {Name}", Progress.Value);
        }

        private int MaxLevelCanBuy()
        {
            return (int)(CostResource.CurrentValue.Value / CostValue);
        }
        
        private void Produce()
        {
            MainThreadDispatcher.StartUpdateMicroCoroutine(WaitForProduce());
        }

        private IEnumerator WaitForProduce()
        {
            while (_progress.Value < 1f)
            {
                _progress.Value += Time.deltaTime / DelayTime;
                yield return null;
            }

            _progress.Value = 0f;
            InvokeEnded();
            ProductionResource.Increase(ProductionValue);
        }

        private void InvokeEnded()
        {
            OnEnd?.Execute(ProductionValue);
        }
    }
}