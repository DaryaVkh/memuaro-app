import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NzPageHeaderModule } from 'ng-zorro-antd/page-header';
import { NzAvatarModule } from 'ng-zorro-antd/avatar';
import { SvgIconComponent } from '../svg-icon/svg-icon.component';
import { Observable } from 'rxjs';
import { UserDto } from '../../../api/api.models';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, NzPageHeaderModule, NzAvatarModule, SvgIconComponent],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HeaderComponent {
  @Input() user$?: Observable<UserDto>;
}
