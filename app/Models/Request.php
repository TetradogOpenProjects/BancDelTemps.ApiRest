<?php

namespace App\Models;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\SoftDeletes;

class Request extends Model
{
    use HasFactory;
    use SoftDeletes;
    public function User(){
        return $this->belongsTo(User::class);
    }
    public function ApprovedBy(){
        return $this->belongsTo(User::class,'approvedBy_id');
    }
    public function Location(){
        return $this->belongsTo(Location::class);
    }
}
