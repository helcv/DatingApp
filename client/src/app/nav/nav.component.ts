import { Component, OnInit, NgModule, Injector } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { Observable, of } from 'rxjs';
import { User } from '../_models/user';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { MembersService } from '../_services/members.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',

  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  
  model: any = {};
  isCollapsed = false;

  constructor (public accountService: AccountService, private router: Router, private toasrt: ToastrService, private injector: Injector) {

  }

  ngOnInit(): void {
    
  }

  login(){
    this.accountService.login(this.model).subscribe({
      next: () => {
        const membersService = this.injector.get(MembersService);
        membersService.updateCurrentUser();

        this.router.navigateByUrl('/members');
      }
      //error: error => this.toasrt.error(error.error)
    })
  }

  logout(){
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }

  toggleNavbar(){
    this.isCollapsed = !this.isCollapsed;
  }
}
