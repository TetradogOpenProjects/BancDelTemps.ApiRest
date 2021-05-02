<?php

namespace App\Models;
use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\SoftDeletes;

class EventTransaction extends Model
{
    use HasFactory;
    use SoftDeletes;
    protected $table="EventTransactions";
    public function Event(){
        return $this->belongsTo(Event::class);
    }
    public function Transaction(){
        return $this->belongsTo(Transaction::class);
    }
}
