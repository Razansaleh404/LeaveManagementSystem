# API Endpoints

## Employees

- `GET /api/employees`
- `GET /api/employees/{id}`
- `GET /api/employees/search?term=`
- `POST /api/employees`
- `PUT /api/employees/{id}`
- `DELETE /api/employees/{id}`

## Leave types

- `GET /api/leavetypes`

## Leave requests

- `GET /api/leaverequests`
- `GET /api/leaverequests/{id}`
- `GET /api/leaverequests/filter?status=&fromDate=&toDate=`
- `GET /api/leaverequests/pending`
- `POST /api/leaverequests`
- `PUT /api/leaverequests/{id}/approve`
- `PUT /api/leaverequests/{id}/reject`
