import { NgModule } from '@angular/core';
import { CampaignTypePipe } from './CampaignTypePipe';
import { SecurePipe } from './SecurePipe';
import { NoSanitizePipe } from './NoSanitizePipe';


@NgModule({
    imports: [
        // dep modules
    ],
    declarations: [
        SecurePipe,
        CampaignTypePipe,
        NoSanitizePipe
    ],
    exports: [
        SecurePipe,
        CampaignTypePipe,
        NoSanitizePipe
    ]
})
export class SharedPipesModule { }
