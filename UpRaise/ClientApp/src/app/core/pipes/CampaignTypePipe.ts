import { Pipe, PipeTransform } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CampaignTypes } from '../../shared/enums';

@Pipe({
  name: 'campaignType'
})
export class CampaignTypePipe implements PipeTransform {

  constructor() { }

  transform(campaignTypeId: number) {
      switch (campaignTypeId) {
          case CampaignTypes.Organization:
              return "Organization";

          case CampaignTypes.People:
              return "People";

          default:
              return "";
      }
   
  }
}
