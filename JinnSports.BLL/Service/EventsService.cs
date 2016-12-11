﻿using System.Collections.Generic;
using JinnSports.BLL.Dtos;
using JinnSports.BLL.Interfaces;
using JinnSports.DAL.Repositories;
using JinnSports.DataAccessInterfaces.Interfaces;
using JinnSports.Entities.Entities;
using System.Linq;

namespace JinnSports.BLL.Service
{
    public class EventsService : IEventService
    {
        private IUnitOfWork dataUnit; 
        
        public IDictionary<string, List<SportEventDto>> GetSportEvents()
        {
            IDictionary<string, List<SportEventDto>> orderedEvents = new Dictionary<string, List<SportEventDto>>();
            
            using (dataUnit = new EFUnitOfWork("SportsContext"))
            {
                IEnumerable<SportEvent> sportEvents = dataUnit.Set<SportEvent>().GetAll();
                IEnumerable<SportType> sportTypes = dataUnit.Set<SportType>().GetAll();

                foreach (SportType sportType in sportTypes)
                {
                    orderedEvents.Add(sportType.Name, new List<SportEventDto>());
                }

                foreach (SportEvent sportEvent in sportEvents)
                {
                    SportEventDto eventDto = new SportEventDto();
                    eventDto.Date = sportEvent.Date;

                    foreach (Result result in sportEvent.Results)
                    {
                        eventDto.Results.Add(result.Team.Name, result.Score);
                    }

                    List<SportEventDto> listedEventDtos;
                    if (orderedEvents.TryGetValue(sportEvent.SportType.Name, out listedEventDtos))
                    {
                        listedEventDtos.Add(eventDto);
                    }
                }
            }

            return orderedEvents;
        }

        public void SortEventsByDate(IDictionary<string, List<SportEventDto>> orderedEvents)
        {
            ICollection<List<SportEventDto>> eventsLists = orderedEvents.Values;
            foreach (List<SportEventDto> eventsList in eventsLists)
            {
                eventsList.OrderBy(e => e.Date);
            }
        }
    }
}
