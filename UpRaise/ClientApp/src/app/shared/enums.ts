export enum CampaignTypes {
    Any = 0, // for filtering
    Organization = 1,
    People = 2,
}


export enum CampaignStatuses {
    Disabled = 0,
    Active = 1,
    PendingAcceptance = 2,
}


export enum DateRanges {
    All = 0,
    LastWeek = 1,
    LastMonth = 2,
    LastYear = 3,
    YTD = 4
}

export enum CampaignRedlineEventTypes {
    Created = 1,
    LookedAtBy = 2,
    ChangeRequest = 3,
    Accepted = 4,
    Rejected = 5,
}


export enum CampaignViewTypes {
    List = 1,
    Map = 2,
}


export enum Countries {
    Canada = 1,
    USA = 2,
}
