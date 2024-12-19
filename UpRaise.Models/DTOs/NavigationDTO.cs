using System;
using System.Collections.Generic;

namespace UpRaise.DTOs
{
    public class NavigationDTO
    {
        public NavigationDTO()
        {
            Children = new List<NavigationDTO>();
            Active = false;
        }

        public enum Types {aside, basic, collapsable, divider, group, spacer};

        public string Id { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }

        public string Type { get; set; }

        public bool Hidden { get; set; }
        public bool Active { get; set; }
        public bool Disabled { get; set; }
        public string Link { get; set; }
        public bool ExternalLink { get; set; }
        public bool ExactMatch { get; set; }
        //isActiveMatchOptions?: IsActiveMatchOptions;

        //function?: (item: FuseNavigationItem) => void;

        public NavigationClassesDTO Classes { get; set; }

        public string Icon { get; set; }

        public NavigationBadgeDTO Badge { get; set; }

        public List<NavigationDTO> Children { get; set; }
        public string Meta { get; set; }
    }

    public class NavigationBadgeDTO
    {
        public string Title { get; set; }
        public string Classes { get; set; }
    }

    public class NavigationClassesDTO
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Icon { get; set; }
        public string Wrapper { get; set; }

    }
}