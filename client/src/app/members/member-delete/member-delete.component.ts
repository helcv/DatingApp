import { Component, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { User } from 'src/app/_models/user';
import { Member } from '../../_models/member';
import { AccountService } from '../../_services/account.service';
import { MembersService } from '../../_services/members.service';
import { take } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';

@Component({
  selector: 'app-member-delete',
  templateUrl: './member-delete.component.html',
  styleUrls: ['./member-delete.component.css']
})
export class MemberDeleteComponent implements OnInit{
  @ViewChild('deleteForm') deleteForm: NgForm | undefined;

  model: any = {};

  constructor(private accountService: AccountService, private memberService: MembersService, private toastr: ToastrService, private router: Router) {

  }

  ngOnInit(): void {
    
  }

  deleteMember() {
    const confirmed = window.confirm('Are you sure you want to delete your account?');
    if (confirmed) {
      this.memberService.deleteMember(this.deleteForm?.value).subscribe({
        next: _ => {
          this.toastr.success('Account deleted successfully!');
          this.accountService.logout();
          //this.router.navigate(['/']);
        },
        error: err => {
          console.error('Error occurred while deleting account:', err);
        }
      });
      
    }
  }
}
