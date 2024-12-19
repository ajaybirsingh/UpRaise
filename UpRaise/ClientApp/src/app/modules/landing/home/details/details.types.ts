import { CampaignTypes } from "../../../../shared/enums";
import { CampaignAnalyticTypes } from "./details.enums";

export interface ICampaignAnalyticRequestDTO {
    id: number;
    campaignType: CampaignTypes;
    analyticType: CampaignAnalyticTypes;
}


export interface ICampaignFollowDTO {
    id: number;
    campaignType: CampaignTypes;
    isFollowing: boolean;
}

