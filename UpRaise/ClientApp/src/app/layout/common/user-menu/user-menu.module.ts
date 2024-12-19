import { NgModule } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { UserMenuComponent } from 'app/layout/common/user-menu/user-menu.component';
import { SharedModule } from 'app/shared/shared.module';
import { SharedPipesModule } from 'app/core/pipes/SharedPipesModule'
@NgModule({
    declarations: [
        UserMenuComponent
    ],
    imports     : [
        MatButtonModule,
        MatDividerModule,
        MatIconModule,
        MatMenuModule,
        SharedModule,
        SharedPipesModule
    ],
    exports     : [
        UserMenuComponent
    ]
})
export class UserMenuModule
{
}
