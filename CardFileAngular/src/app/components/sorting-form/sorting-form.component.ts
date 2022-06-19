import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TextMaterialParameters, TextMaterialParams } from 'src/app/models/parameters/TextMaterialParameters';
import { AuthService } from 'src/app/services/auth.service';
import { SharedHomeParamsService } from 'src/app/services/shared-home-params.service';
import { SharedParamsService } from 'src/app/services/shared-params.service';
import { TextMaterialService } from 'src/app/services/text-material.service';


@Component({
  selector: 'app-sorting-form',
  templateUrl: './sorting-form.component.html',
  styleUrls: ['./sorting-form.component.css']
})
export class SortingFormComponent implements OnInit {
  sortingParamsForm: FormGroup;
  textMaterialParams: TextMaterialParams = new TextMaterialParams();
  isManager: boolean;
  isAdmin: boolean;

  //@Input() userId: string;
  @Input() isHomePage: boolean;

  @Output() filter : EventEmitter<TextMaterialParameters> = new EventEmitter<TextMaterialParameters>();

  constructor(private fb: FormBuilder,
    private authService: AuthService,
    private sharedParams: SharedParamsService,
    private sharedHomeParams: SharedHomeParamsService,
    private datePipe: DatePipe) { }

  ngOnInit(): void {
    if (this.isHomePage){
      this.createHomeForm();
    }
    else{
      this.createForm();
    }

    if (this.isHomePage){
      this.configureHomeTextMaterialParams();
    }
    else{
      this.configureTextMaterialParams();
    }

    this.authService.claims.subscribe( c => {
      if (c){
        this.isManager = c.includes('Manager');
      }
    });
  }

  configureTextMaterialParams(){
    this.textMaterialParams.orderBy = this.sharedParams.orderBy;
    this.textMaterialParams.filterFromDate = this.sharedParams.filterFromDate;
    this.textMaterialParams.filterToDate = this.sharedParams.filterToDate;

    if (!this.isManager){
      this.textMaterialParams.approvalStatus = [];
      this.textMaterialParams.approvalStatus.push(1);
    }
    else if (!this.isAdmin){
      this.textMaterialParams.approvalStatus = [];
      this.textMaterialParams.approvalStatus.push(0,1);
    }
    else{
      this.textMaterialParams.approvalStatus = this.sharedParams.approvalStatus;
    }

    this.textMaterialParams.approvalStatus = this.sharedParams.approvalStatus;
    this.textMaterialParams.searchTitle = this.sharedParams.searchTitle;
    this.textMaterialParams.searchCategory = this.sharedParams.searchCategory;
    this.textMaterialParams.searchAuthor = this.sharedParams.searchAuthor;
    // this.textMaterialParams.pageNumber = this.sharedParams.pageNumber;
    // this.textMaterialParams.pageSize = 2;
  }

  configureHomeTextMaterialParams(){
    this.textMaterialParams.orderBy = this.sharedHomeParams.orderBy;
    this.textMaterialParams.filterFromDate = this.sharedHomeParams.filterFromDate;
    this.textMaterialParams.filterToDate = this.sharedHomeParams.filterToDate;

    if (!this.isManager){
      this.textMaterialParams.approvalStatus = [];
      this.textMaterialParams.approvalStatus.push(1);
    }
    else if (!this.isAdmin){
      this.textMaterialParams.approvalStatus = [];
      this.textMaterialParams.approvalStatus.push(0,1);
    }
    else{
      this.textMaterialParams.approvalStatus = this.sharedHomeParams.approvalStatus;
    }

    this.textMaterialParams.approvalStatus = this.sharedHomeParams.approvalStatus;
    this.textMaterialParams.searchTitle = this.sharedHomeParams.searchTitle;
    this.textMaterialParams.searchCategory = this.sharedHomeParams.searchCategory;
    this.textMaterialParams.searchAuthor = this.sharedHomeParams.searchAuthor;
    // this.textMaterialParams.pageNumber = this.sharedParams.pageNumber;
    // this.textMaterialParams.pageSize = 2;
  }

  createForm(){
    this.sortingParamsForm = this.fb.group({
      sortByTitle: [null],
      sortByCategory: [null],
      sortByDatePublished: [null],
      filterFromDate: [this.sharedParams.filterFromDate],
      filterToDate: [this.sharedParams.filterToDate],
      pending: [this.sharedParams.approvalStatus.includes(0)],
      approved: [this.sharedParams.approvalStatus.includes(1)],
      rejected: [this.sharedParams.approvalStatus.includes(2)],
      searchTitle: [this.sharedParams.searchTitle],
      searchCategory: [this.sharedParams.searchCategory],
      searchAuthor: [this.sharedParams.searchAuthor],
    });
  }

  createHomeForm(){
    this.sortingParamsForm = this.fb.group({
      sortByTitle: [null],
      sortByCategory: [null],
      sortByDatePublished: [null],
      filterFromDate: [this.sharedHomeParams.filterFromDate],
      filterToDate: [this.sharedHomeParams.filterToDate],
      pending: [this.sharedHomeParams.approvalStatus.includes(0)],
      approved: [this.sharedHomeParams.approvalStatus.includes(1)],
      rejected: [this.sharedHomeParams.approvalStatus.includes(2)],
      searchTitle: [this.sharedHomeParams.searchTitle],
      searchCategory: [this.sharedHomeParams.searchCategory],
      searchAuthor: [this.sharedHomeParams.searchAuthor],
    });
  }

  validateFromDate(){
    let currentDate = new Date();
    var fromDate = new Date(this.sortingParamsForm.get('filterFromDate').value);
    var toDate = new Date(this.sortingParamsForm.get('filterToDate').value);

    if ((fromDate.getTime() > toDate.getTime() && this.sortingParamsForm.get('filterToDate').value) ||
        fromDate.getTime() > currentDate.getTime()){
      this.sortingParamsForm.get('filterFromDate').setValue(this.datePipe.transform(currentDate,'yyyy-MM-dd'));
    }
  }

  validateToDate(){
    let currentDate = new Date();

    var fromDate = new Date(this.sortingParamsForm.get('filterFromDate').value);
    var toDate = new Date(this.sortingParamsForm.get('filterToDate').value);

    if (toDate.getTime() < fromDate.getTime() ||
        toDate.getTime() > currentDate.getTime()){
      this.sortingParamsForm.get('filterToDate').setValue(this.datePipe.transform(currentDate,'yyyy-MM-dd'));
    }
  }

  onSubmit(){
    this.textMaterialParams.filterFromDate = this.sortingParamsForm.get('filterFromDate').value;

    this.textMaterialParams.filterToDate = this.sortingParamsForm.get('filterToDate').value;

    this.textMaterialParams.approvalStatus = [];

    if (this.sortingParamsForm.get('pending').value){
      this.textMaterialParams.approvalStatus.push(0);
    }

    if (this.sortingParamsForm.get('approved').value){
      this.textMaterialParams.approvalStatus.push(1);
    }

    if (this.sortingParamsForm.get('rejected').value){
      this.textMaterialParams.approvalStatus.push(2);
    }

    this.textMaterialParams.orderBy = '';
    if (this.sortingParamsForm.get('sortByTitle').value != null){
      if (this.sortingParamsForm.get('sortByTitle').value){
        this.textMaterialParams.orderBy += 'title asc,';
      }
      else{
        this.textMaterialParams.orderBy += 'title desc,';
      }
    }

    if (this.sortingParamsForm.get('sortByCategory').value != null){
      if (this.sortingParamsForm.get('sortByCategory').value){
        this.textMaterialParams.orderBy += 'category asc';
      }
      else{
        this.textMaterialParams.orderBy += 'category desc';
      }
    }

    this.textMaterialParams.searchTitle = this.sortingParamsForm.get('searchTitle').value;

    this.textMaterialParams.searchCategory = this.sortingParamsForm.get('searchCategory').value;

    this.textMaterialParams.searchAuthor = this.sortingParamsForm.get('searchAuthor').value;

    if (this.isHomePage){
      this.textMaterialParams.pageNumber = this.sharedHomeParams.pageNumber;
      this.textMaterialParams.pageSize = this.sharedHomeParams.pageSize;
    }else{
      this.textMaterialParams.pageNumber = this.sharedParams.pageNumber;
      this.textMaterialParams.pageSize = this.sharedParams.pageSize;
    }

    this.filter.emit(this.textMaterialParams);
  }
}
