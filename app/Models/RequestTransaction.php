<?php

namespace App\Models;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\SoftDeletes;

class RequestTransaction extends Model
{
    use HasFactory;
    use SoftDeletes;
    protected $table="RequestTransactions";
    public function Request(){
        return $this->belongsTo(Request::class);
    }
    public function Transaction(){
        return $this->belongsTo(Transaction::class);
    }
}
