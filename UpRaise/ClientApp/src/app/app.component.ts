import { Component } from '@angular/core';
import { ApplicationInsightsService } from './core/applicationinsights/applicationinsights.service';

@Component({
    selector   : 'app-root',
    templateUrl: './app.component.html',
    styleUrls  : ['./app.component.scss']
})

export class AppComponent
{
    /**
     * Constructor
     */
    constructor(private ApplicationInsightsService: ApplicationInsightsService)
    {
    }

    ngOnInit() {
    }
}
