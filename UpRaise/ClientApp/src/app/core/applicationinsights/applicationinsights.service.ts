import { Injectable } from '@angular/core';
import { ApplicationInsights, IExceptionTelemetry, DistributedTracingModes } from '@microsoft/applicationinsights-web';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { environment } from 'environments/environment';

@Injectable({
    providedIn: 'root',
})

    //
    //https://onthecode.co.uk/monitoring-live-angular-apps-with-azure-application-insights/
    //
export class ApplicationInsightsService {
    private appInsights: ApplicationInsights;

    constructor(private router: Router) {
        this.appInsights = new ApplicationInsights({
            config: {
                instrumentationKey: environment.appInsights.instrumentationKey,
                //enableAutoRouteTracking: true // option to log all route changes
            }
        });

        this.appInsights.loadAppInsights();
        this.loadCustomTelemetryProperties();
        this.createRouterSubscription();
    }

    setUserId(userId: string) {
        this.appInsights.setAuthenticatedUserContext(userId);
    }

    clearUserId() {
        this.appInsights.clearAuthenticatedUserContext();
    }

    logPageView(name?: string, uri?: string) { // option to call manually
        this.appInsights.trackPageView({ name, uri });
    }

    logException(error: Error) {
        let exception: IExceptionTelemetry = {
            exception: error
        };
        this.appInsights.trackException(exception);
    }

    private loadCustomTelemetryProperties() {
        this.appInsights.addTelemetryInitializer(envelope => {
            var item = envelope.baseData;
            item.properties = item.properties || {};
            item.properties["ApplicationPlatform"] = "WEB";
            item.properties["ApplicationName"] = "UpRaise";
        }
        );
    }

    private createRouterSubscription() {
        this.router.events.pipe(filter(event => event instanceof NavigationEnd)).subscribe((event: NavigationEnd) => {
            this.logPageView(null, event.urlAfterRedirects);
        });
    }
}
