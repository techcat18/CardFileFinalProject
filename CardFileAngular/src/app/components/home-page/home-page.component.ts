import { Component, OnInit } from '@angular/core';
import { TextMaterialParameters, TextMaterialParams } from 'src/app/models/parameters/TextMaterialParameters';
import { TextMaterial } from 'src/app/models/TextMaterial';
import { AuthService } from 'src/app/services/auth.service';
import { TextMaterialService } from 'src/app/services/text-material.service';

@Component({
  selector: 'app-home-page',
  templateUrl: './home-page.component.html',
  styleUrls: ['./home-page.component.css']
})
export class HomePageComponent implements OnInit {
  ownTextMaterials: TextMaterial[];
  showApproved: string;

  userId: string;
  userName: string;

  constructor(private authService: AuthService,
    private textMaterialService: TextMaterialService) { }

  ngOnInit(): void {
    this.authService.getUserInfo().subscribe( u => {
      if (u){
        this.userId = u.sub;
        this.userName = u.name;
      }
    });

    this.getOwnTextMaterials();
  }

  getAllTextMaterials(){
    this.textMaterialService.showApproved.next(null);
  }

  getApprovedMaterials(){
    this.textMaterialService.showApproved.next(true.toString());
  }

  getRejectedMaterials(){
    this.textMaterialService.showApproved.next(false.toString());
  }

  getOwnTextMaterials(){
    this.textMaterialService.getTextMaterialsByUserId(this.userId, new TextMaterialParams()).subscribe(tm => {
      this.ownTextMaterials = tm;
    }, err => {
      console.log(err);
    });
  }

  onFilter(parameters: TextMaterialParameters){
    this.textMaterialService.getTextMaterialsByUserId(this.userId, parameters).subscribe( tm => {
      this.ownTextMaterials = tm;
    }, err => {
      console.log(err);
    });
  }
}
