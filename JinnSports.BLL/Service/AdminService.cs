﻿using AutoMapper;
using JinnSports.BLL.Dtos;
using JinnSports.BLL.Interfaces;
using JinnSports.DataAccessInterfaces.Interfaces;
using JinnSports.Entities.Entities;
using JinnSports.Entities.Entities.Temp;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JinnSports.BLL.Service
{
    public class AdminService : IAdminService
    {
        private static readonly ILog Log = LogManager.GetLogger("AppLog");

        private readonly IUnitOfWork dataUnit;

        public AdminService(IUnitOfWork unitOfWork)
        {
            this.dataUnit = unitOfWork;
        }

        public List<ConformityDto> GetConformities()
        {
            Log.Info("Getting conformities data...");
            List<ConformityDto> inputNames = new List<ConformityDto>();
            IEnumerable<Conformity> conformities = new List<Conformity>();

            try
            {
                conformities = this.dataUnit.GetRepository<Conformity>()
                    .Get().GroupBy(x => x.InputName)
                    .Select(x => x.First())
                    .ToList();
            }
            catch (Exception e)
            {
                Log.Error("Exception when trying to get conformities data from DB", e);
            }

            foreach (Conformity conf in conformities)
            {
                inputNames.Add(Mapper.Map<Conformity, ConformityDto>(conf));
            }

            Log.Info("Successful getting conformities data...");

            return inputNames;
        }

        public AdminApiViewModel GetConformityApiViewModel()
        {
            Log.Info("Getting conformities data...");
            IList<ConformityDto> conformityDtos = new List<ConformityDto>();
            IEnumerable<Conformity> conformities = new List<Conformity>();

            try
            {
                conformities = this.dataUnit.GetRepository<Conformity>().Get();
            }
            catch (Exception e)
            {
                Log.Error("Exception when trying to get conformities data from DB", e);
            }

            foreach (Conformity conformity in conformities)
            {
                conformityDtos.Add(Mapper.Map<Conformity, ConformityDto>(conformity));
            }

            List<ConformityApiViewModel> groups = conformityDtos
                .GroupBy(c => c.InputName)
                .Select(g => new ConformityApiViewModel() { GroupName = g.Key, Dtos = g.ToList() })
                .ToList();

            List<string> names = this.dataUnit.GetRepository<TeamName>()
                .Get()
                .Select(x => x.Name)
                .ToList();

            Log.Info("Successful getting conformities data...");
           
            return new AdminApiViewModel { Conformities = groups, Names = names };
        }        

        public ConformityViewModel GetConformityViewModel(int id)
        {
            Log.Info("Getting ConformityViewModel...");

            Conformity confByID = new Conformity();
            IEnumerable<Conformity> conformities = new List<Conformity>();

            try
            {
                confByID = this.dataUnit.GetRepository<Conformity>()
                    .Get(x => x.Id == id).FirstOrDefault();

                conformities = this.dataUnit.GetRepository<Conformity>()
                    .Get(x => x.InputName == confByID.InputName)
                    .GroupBy(x => x.ExistedName)
                    .Select(x => x.First());
            }
            catch (Exception e)
            {
                Log.Error("Exception when trying to get conformities data from DB", e);
            }

            List<ConformityDto> conformitiesDtos = new List<ConformityDto>();

            foreach (Conformity conf in conformities)
            {
                conformitiesDtos.Add(Mapper.Map<Conformity, ConformityDto>(conf));
            }

            Log.Info("Successful getting ConformityViewModel...");
            return new ConformityViewModel(confByID.InputName, confByID.Id, conformitiesDtos);
        }

        public void Save(ConformityViewModel model)
        {
            Log.Info("Managing ConformityViewModel post...");
            try
            {
                if (model.ConformityId != 0)
                {
                    Conformity conf = this.dataUnit.GetRepository<Conformity>()
                        .Get(x => x.Id == model.ConformityId)
                        .FirstOrDefault();

                    this.Saving(conf.InputName, conf.ExistedName, false);
                }
                if (model.ExistedName != null)
                {
                    if (this.CheckExistedNamesInDB(model.ExistedName))
                    {
                        this.Saving(model.InputName, model.ExistedName, false);
                    }
                    else
                    {
                        this.Saving(model.InputName, model.ExistedName, true);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception when trying to manage ConformityViewModel and DB", e);
            }
            Log.Info("Successful manaing ConformityViewModel...");
        }

        public void Save(string inputName, string existedName)
        {
            Log.Info("Managing ConformityViewModel post...");
            try
            {
                if (this.CheckExistedNamesInDB(existedName))
                {
                    this.Saving(inputName, existedName, false);
                }
                else
                {
                    this.Saving(inputName, existedName, true);
                }
            }
            catch (Exception e)
            {
                Log.Error("Exception when trying to manage ConformityViewModel and DB", e);
            }
            Log.Info("Successful manaing ConformityViewModel...");
        }

        private bool CheckExistedNamesInDB(string existedName)
        {
            IEnumerable<TeamName> namesInDB = this.dataUnit.GetRepository<TeamName>()
                .Get(x => x.Name == existedName);

            if (namesInDB.Count() != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Saving(string inputName, string existedName, bool createNewTeam)
        {
            IEnumerable<Conformity> conformities = this.dataUnit.GetRepository<Conformity>()
                .Get(filter: x => x.InputName == inputName, includeProperties: "TempResult")
                .GroupBy(x => x.TempResult.Id)
                .Select(x => x.First());

            foreach (Conformity conf in conformities)
            {
                TempSportEvent tempEvent = this.dataUnit.GetRepository<TempResult>()
                    .Get(filter: x => x.Id == conf.TempResult.Id, includeProperties: "TempSportEvent")
                    .Select(x => x.TempSportEvent)
                    .FirstOrDefault();

                Team team = this.GetTeam(inputName, existedName, createNewTeam, tempEvent);
                
                int resultId1 = tempEvent.TempResults.ToArray()[0].Id;
                int resultId2 = tempEvent.TempResults.ToArray()[1].Id;
                var res1 = this.dataUnit.GetRepository<TempResult>()
                    .Get(filter: x => x.Id == resultId1, includeProperties: "Team")
                    .FirstOrDefault();

                var res2 = this.dataUnit.GetRepository<TempResult>()
                    .Get(filter: x => x.Id == resultId2, includeProperties: "Team")
                    .FirstOrDefault();   

                if (res1.Team != null || res2.Team != null)
                {
                    tempEvent.TempResults = null;
                    tempEvent.TempResults = new List<TempResult> { res1, res2 };
                    
                    conf.TempResult.Team = team;
                    this.dataUnit.GetRepository<SportEvent>()
                        .Insert(Mapper.Map<TempSportEvent, SportEvent>(tempEvent));

                    this.DeleteTempEvent(tempEvent);                 

                    this.dataUnit.SaveChanges();
                }
                else
                {
                    conf.TempResult.Team = team;

                    this.DeleteConformities(conf.TempResult);

                    this.dataUnit.SaveChanges();
                }
            }
        }

        private Team GetTeam(string inputName, string existedName, bool createNewTeam, TempSportEvent tempEvent)
        {
            TeamName tn = new TeamName { Name = inputName };
            Team team = new Team();

            if (createNewTeam)
            {
                TeamName tn2 = new TeamName { Name = existedName };
                team = new Team
                {
                    Name = existedName,
                    SportType = tempEvent.SportType,
                    Names = new List<TeamName> { tn }
                };
                if (inputName != existedName)
                {
                    team.Names.Add(tn2);
                }
            }
            else
            {
                team = this.dataUnit.GetRepository<TeamName>()
                    .Get(filter: t => t.Name == existedName, includeProperties: "Team")
                    .Select(t => t.Team)
                    .FirstOrDefault();

                team.Names.Add(tn);
            }

            return team;
        }

        private void DeleteConformities(TempResult tempResult)
        {
            int j = tempResult.Conformities.Count;
            var array = tempResult.Conformities.ToArray();

            for (int i = 0; i != j; i++)
            {
                this.dataUnit.GetRepository<Conformity>().Delete(array[i]);
            }
        }

        private void DeleteTempEvent(TempSportEvent tempEvent)
        {
            int resultsCount = tempEvent.TempResults.Count;
            var resultsArray = tempEvent.TempResults.ToArray();

            for (int j = 0; j != resultsCount; j++)
            {
                if (resultsArray[j].Conformities != null)
                {
                    int confCount = resultsArray[j].Conformities.Count;
                    var confArray = resultsArray[j].Conformities.ToArray();

                    for (int i = 0; i != confCount; i++)
                    {
                        this.dataUnit.GetRepository<Conformity>().Delete(confArray[i]);
                    } 
                }
                this.dataUnit.GetRepository<TempResult>().Delete(resultsArray[j]);
            }
            this.dataUnit.GetRepository<TempSportEvent>().Delete(tempEvent);
        }     
    }
}
