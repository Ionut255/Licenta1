﻿using Sources.Architecture.Interfaces;
using Sources.Data;
using UniRx;
using UnityEngine;

namespace Sources.Models
{
    public class Manager : IManager, IDeInitiable
    {
        public string Name { get; }
        public string Description { get; }
        public Sprite Icon { get; }
        public IGenerator Generator => _generator;
        public bool IsActive => _isActive;
        public double CostValue { get; }
        public IResource CostResource { get; }
        public double CostValue1 { get; }
        public IResource CostResource1 { get; }
        public double CostValue2 { get; }
        public IResource CostResource2 { get; }


        private bool _isActive;
        private readonly IGenerator _generator;
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        private Manager(IGenerator generator, IResource costResource, ManagerData data, bool buyed)
        {
            _generator = generator;
            CostValue = data.Value;
            CostValue1 = data.Value;
            CostValue2 = data.Value;
            CostResource = costResource;
            CostResource1 = costResource;
            CostResource2 = costResource;
            Name = data.Name;
            Description = data.Description;
            Icon = data.Icon;
            ChangeActive(buyed);
        }

        private Manager(IGenerator generator)
        {
            _generator = generator;
        }

        public static Manager Load(ManagerData data, IGenerator generator, IResource costResource)
        {
            var isBuyed = bool.Parse(PlayerPrefs.GetString($"Manager: {data.Name}", false.ToString()));
            return new Manager(generator, costResource, data, isBuyed);
        }

        public static IManager CreateMock(IGenerator generator)
        {
            return new Manager(generator);
        }

        public void ChangeActive(bool value)
        {
            _isActive = value;
            if (IsActive)
            {
                _generator.OnEnd.Subscribe(GeneratorOnEnded).AddTo(_compositeDisposable);
                _generator.TryProduce();
            }
            else
            {
                _compositeDisposable.Clear();
            }
        }

        public void Save()
        {
            PlayerPrefs.SetString($"Manager: {Name}", IsActive.ToString());
        }
        
        public void DeInit()
        {
            _compositeDisposable.Clear();
        }
        
        private void GeneratorOnEnded(double value)
        {
            _generator.TryProduce();
        }
    }
}