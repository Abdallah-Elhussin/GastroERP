import { Component, ChangeDetectionStrategy, inject, signal, OnInit, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { DataService } from '../../core/services/data.service';
import { LanguageService } from '../../core/services/language.service';
import { Employee } from '../../core/models/erp.models';
import { HrRepository } from '../../core/repositories/hr.repository';
import { AppButtonComponent } from '../../shared/ui/app-button/app-button.component';
import { AppCardComponent } from '../../shared/ui/app-card/app-card.component';
import { AppTableComponent } from '../../shared/ui/app-table/app-table.component';

@Component({
  selector: 'app-employees',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatIconModule,
    AppButtonComponent,
    AppCardComponent,
    AppTableComponent
  ],
  templateUrl: './employees.component.html',
  styleUrl: './employees.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EmployeesComponent implements OnInit {
  private fb = inject(FormBuilder);
  dataService = inject(DataService);
  langService = inject(LanguageService);
  hrRepo = inject(HrRepository);

  mainView = signal<'list' | 'profile'>('list');
  showEmployeeForm = signal(false);

  employees = signal<Employee[]>([...this.dataService.employees]);
  activeTab = signal<string>('emp.personalInfo');
  tabKeys = ['emp.personalInfo', 'emp.attendance', 'emp.leaves', 'emp.contracts', 'emp.performance'];
  employee = signal<Employee>(this.dataService.employees[0]);

  employeeForm = this.fb.group({
    name: ['', Validators.required],
    title: ['', Validators.required],
    department: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', Validators.required]
  });

  columns = computed(() => {
    this.langService.language();
    return [
      { key: 'name', label: this.t('hr.col.name'), sortable: true },
      { key: 'title', label: this.t('hr.col.title'), sortable: true },
      { key: 'department', label: this.t('hr.col.department'), sortable: true },
      { key: 'email', label: this.t('hr.col.email'), sortable: true },
      { key: 'actions', label: this.t('menu.col.actions'), sortable: false }
    ];
  });

  departments = [
    { key: 'emp.dept.fnb', count: 24 },
    { key: 'emp.dept.kitchen', count: 18, active: true },
    { key: 'emp.dept.logistics', count: 8 }
  ];

  teamMembers = computed(() =>
    this.employees().map(e => ({
      employee: e,
      name: e.name,
      title: e.title,
      avatar: e.avatar,
      active: this.employee().name === e.name
    }))
  );

  ngOnInit(): void {
    this.hrRepo.getEmployees().subscribe(list => {
      if (list?.length) {
        this.employees.set(list);
        this.employee.set(list[0]);
      }
    });
  }

  openAddEmployee(): void {
    this.employeeForm.reset();
    this.showEmployeeForm.set(true);
  }

  saveEmployee(): void {
    if (this.employeeForm.invalid) {
      this.employeeForm.markAllAsTouched();
      return;
    }

    const v = this.employeeForm.getRawValue();
    const newEmployee: Employee = {
      id: `EMP-${Date.now()}`,
      name: v.name!,
      title: v.title!,
      department: v.department!,
      email: v.email!,
      phone: v.phone!,
      address: '',
      hireDate: new Date(),
      tenure: '0.0 yrs',
      performance: '—',
      attendance: '—',
      avatar: 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=80&auto=format&fit=crop&q=80',
      leaveBalances: [],
      summary: '',
      performanceNotes: ''
    };

    this.employees.update(list => [...list, newEmployee]);
    this.dataService.employees.push(newEmployee);
    this.showEmployeeForm.set(false);
  }

  selectEmployee(emp: Employee): void {
    this.employee.set(emp);
    this.mainView.set('profile');
    this.activeTab.set('emp.personalInfo');
  }

  t(key: string): string {
    return this.langService.t(key);
  }
}
